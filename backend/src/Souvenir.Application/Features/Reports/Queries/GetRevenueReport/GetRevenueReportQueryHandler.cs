using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Reports;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Reports.Queries.GetRevenueReport;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetRevenueReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueSummaryDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var (fromDate, toDate, periodName) = ResolveDateRange(request.Period, request.FromDate, request.ToDate, now);

        // Normalize DateTimeKind for Postgres
        var fromUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Unspecified);
        var toUtc = DateTime.SpecifyKind(toDate, DateTimeKind.Unspecified);

        // Query completed orders
        var completedOrders = await _context.DonHangs
            .AsNoTracking()
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Where(d => d.NgayTao >= fromUtc && d.NgayTao <= toUtc)
            .Where(d => d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == "DA_THANH_TOAN")
            .ToListAsync(cancellationToken);

        // Query returns if any
        decimal tongTienHoan = 0m;
        try
        {
            tongTienHoan = await _context.PhieuTraHangs
                .AsNoTracking()
                .Where(p => p.NgayTra >= fromUtc && p.NgayTra <= toUtc)
                .SumAsync(p => p.TongTienHoan, cancellationToken);
        }
        catch
        {
            // Fallback if table is empty or unmigrated in some environments
            tongTienHoan = 0m;
        }

        // Summary calculations
        decimal grossRevenue = completedOrders.Sum(d => d.TongTienThanhToan);
        decimal netRevenue = Math.Max(0, grossRevenue - tongTienHoan);
        int totalOrders = completedOrders.Count;
        int totalItemsSold = completedOrders.SelectMany(d => d.ChiTietDonHangs).Sum(ct => ct.SoLuong);

        // Total discounts
        decimal totalDiscounts = completedOrders.Sum(d =>
        {
            var itemsSum = d.TinhTongTienHang();
            return Math.Max(0, itemsSum - d.TongTienThanhToan);
        });

        // Estimated profit = Net Revenue - Total Cost of Goods Sold
        decimal totalCost = completedOrders
            .SelectMany(d => d.ChiTietDonHangs)
            .Sum(ct => (ct.SanPham != null ? ct.SanPham.GiaNhap : 0m) * ct.SoLuong);
        decimal estimatedProfit = Math.Max(0, netRevenue - totalCost);

        // Inventory totals
        var products = await _context.SanPhams.AsNoTracking().ToListAsync(cancellationToken);
        int totalInventory = products.Sum(p => p.SoLuongTon);
        decimal totalInventoryValue = products.Sum(p => p.SoLuongTon * p.GiaBan);

        // Customer total
        int totalCustomers = await _context.KhachHangs.AsNoTracking().CountAsync(cancellationToken);

        // Build Timeline Points
        var timeline = BuildTimeline(completedOrders, fromDate, toDate, periodName);

        return new RevenueSummaryDto
        {
            TongDoanhThu = netRevenue,
            TongDonHang = totalOrders,
            TongSanPhamDaBan = totalItemsSold,
            TongTienGiam = totalDiscounts,
            TongLoiNhuanUocTinh = estimatedProfit,
            TongTonKho = totalInventory,
            TongGiaTriTonKho = totalInventoryValue,
            TongKhachHang = totalCustomers,
            KyBaoCao = periodName,
            TuNgay = fromDate,
            DenNgay = toDate,
            Timeline = timeline
        };
    }

    private static (DateTime FromDate, DateTime ToDate, string PeriodName) ResolveDateRange(
        string? period, DateTime? customFrom, DateTime? customTo, DateTime now)
    {
        var p = (period ?? "thisMonth").ToLowerInvariant().Trim();

        if (p == "custom" && customFrom.HasValue && customTo.HasValue)
        {
            var start = customFrom.Value.Date;
            var end = customTo.Value.Date.AddDays(1).AddTicks(-1);
            return (start, end, "Tùy chọn");
        }

        switch (p)
        {
            case "today":
                return (now.Date, now.Date.AddDays(1).AddTicks(-1), "Hôm nay");

            case "yesterday":
                var y = now.Date.AddDays(-1);
                return (y, y.AddDays(1).AddTicks(-1), "Hôm qua");

            case "7days":
                var d7 = now.Date.AddDays(-6);
                return (d7, now.Date.AddDays(1).AddTicks(-1), "7 ngày qua");

            case "30days":
                var d30 = now.Date.AddDays(-29);
                return (d30, now.Date.AddDays(1).AddTicks(-1), "30 ngày qua");

            case "lastmonth":
                var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonthEnd = firstOfThisMonth.AddTicks(-1);
                var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);
                return (lastMonthStart, lastMonthEnd, $"Tháng {lastMonthStart.Month}/{lastMonthStart.Year}");

            case "thismonth":
            default:
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
                return (startOfMonth, endOfMonth, $"Tháng {now.Month}/{now.Year}");
        }
    }

    private static List<RevenueTimelinePointDto> BuildTimeline(
        List<DonHang> orders, DateTime fromDate, DateTime toDate, string periodName)
    {
        var result = new List<RevenueTimelinePointDto>();
        var span = toDate - fromDate;

        if (span.TotalDays <= 1.5)
        {
            // Hourly breakdown (in 2-hour blocks: 0-2, 2-4, 4-6, ... 22-24)
            for (int h = 0; h < 24; h += 2)
            {
                var blockStart = fromDate.Date.AddHours(h);
                var blockEnd = blockStart.AddHours(2);
                var matching = orders
                    .Where(o => o.NgayTao >= blockStart && o.NgayTao < blockEnd)
                    .ToList();

                result.Add(new RevenueTimelinePointDto
                {
                    Date = blockStart.ToString("yyyy-MM-dd HH:mm"),
                    Label = $"{h:D2}:00",
                    DoanhThu = matching.Sum(o => o.TongTienThanhToan),
                    SoDonHang = matching.Count,
                    SoLuongBan = matching.SelectMany(o => o.ChiTietDonHangs).Sum(ct => ct.SoLuong)
                });
            }
        }
        else if (span.TotalDays <= 35)
        {
            // Day by day breakdown
            for (var d = fromDate.Date; d <= toDate.Date; d = d.AddDays(1))
            {
                var nextDay = d.AddDays(1);
                var matching = orders
                    .Where(o => o.NgayTao >= d && o.NgayTao < nextDay)
                    .ToList();

                result.Add(new RevenueTimelinePointDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    Label = d.ToString("dd/MM"),
                    DoanhThu = matching.Sum(o => o.TongTienThanhToan),
                    SoDonHang = matching.Count,
                    SoLuongBan = matching.SelectMany(o => o.ChiTietDonHangs).Sum(ct => ct.SoLuong)
                });
            }
        }
        else
        {
            // Month by month breakdown
            var cur = new DateTime(fromDate.Year, fromDate.Month, 1);
            var endMonth = new DateTime(toDate.Year, toDate.Month, 1);

            while (cur <= endMonth)
            {
                var nextMonth = cur.AddMonths(1);
                var matching = orders
                    .Where(o => o.NgayTao >= cur && o.NgayTao < nextMonth)
                    .ToList();

                result.Add(new RevenueTimelinePointDto
                {
                    Date = cur.ToString("yyyy-MM"),
                    Label = cur.ToString("MM/yyyy"),
                    DoanhThu = matching.Sum(o => o.TongTienThanhToan),
                    SoDonHang = matching.Count,
                    SoLuongBan = matching.SelectMany(o => o.ChiTietDonHangs).Sum(ct => ct.SoLuong)
                });

                cur = nextMonth;
            }
        }

        return result;
    }
}
