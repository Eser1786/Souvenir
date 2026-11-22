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

namespace Souvenir.Application.Features.Reports.Queries.GetTopProducts;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, TopProductsReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTopProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TopProductsReportDto> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var (fromDate, toDate) = ResolveDateRange(request.Period, request.FromDate, request.ToDate, now);

        var fromUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Local).ToUniversalTime();
        var toUtc = DateTime.SpecifyKind(toDate, DateTimeKind.Local).ToUniversalTime();

        var limit = request.Limit > 0 ? request.Limit : 5;

        // Fetch completed orders within period
        var completedOrders = await _context.DonHangs
            .AsNoTracking()
            .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
            .Where(d => d.NgayTao >= fromUtc && d.NgayTao <= toUtc &&
                        (d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == "DA_THANH_TOAN"))
            .ToListAsync(cancellationToken);

        var categories = await _context.DanhMucSanPhams
            .AsNoTracking()
            .ToDictionaryAsync(dm => dm.Id, dm => dm.TenDanhMuc, cancellationToken);

        var orderDetails = completedOrders.SelectMany(d => d.ChiTietDonHangs).ToList();

        decimal totalRevenueAll = orderDetails.Sum(ct => ct.ThanhTien);

        var grouped = orderDetails
            .GroupBy(ct => ct.SanPhamId)
            .Select(g =>
            {
                var first = g.First();
                var sp = first.SanPham;
                var soLuong = g.Sum(x => x.SoLuong);
                var revenue = g.Sum(x => x.ThanhTien);
                var cost = g.Sum(x => (sp != null ? sp.GiaNhap : 0m) * x.SoLuong);
                var profit = Math.Max(0, revenue - cost);
                var catName = (sp != null && categories.TryGetValue(sp.DanhMucId, out var cn)) ? cn : "Đồ lưu niệm";

                return new TopProductItemDto
                {
                    SanPhamId = g.Key,
                    MaSP = sp?.MaSP ?? $"SP{g.Key:D3}",
                    TenSP = sp?.TenSP ?? "Sản phẩm",
                    DanhMuc = catName,
                    HinhAnh = sp?.HinhAnh,
                    GiaBan = sp?.GiaBan ?? (soLuong > 0 ? revenue / soLuong : 0m),
                    GiaNhap = sp?.GiaNhap ?? 0m,
                    SoLuongDaBan = soLuong,
                    TongDoanhThu = revenue,
                    LoiNhuanUocTinh = profit,
                    PhanTramDoanhThu = totalRevenueAll > 0 ? Math.Round((double)(revenue / totalRevenueAll) * 100, 1) : 0
                };
            })
            .OrderByDescending(p => p.SoLuongDaBan)
            .ThenByDescending(p => p.TongDoanhThu)
            .Take(limit)
            .ToList();

        // If no sales yet in period, provide sample top products from catalog so dashboard doesn't look empty
        if (grouped.Count == 0)
        {
            var catalogProducts = await _context.SanPhams
                .AsNoTracking()
                .Include(p => p.DanhMuc)
                .Take(limit)
                .ToListAsync(cancellationToken);

            grouped = catalogProducts.Select(p => new TopProductItemDto
            {
                SanPhamId = p.Id,
                MaSP = p.MaSP,
                TenSP = p.TenSP,
                DanhMuc = p.DanhMuc?.TenDanhMuc ?? "Đồ lưu niệm",
                HinhAnh = p.HinhAnh,
                GiaBan = p.GiaBan,
                GiaNhap = p.GiaNhap,
                SoLuongDaBan = 0,
                TongDoanhThu = 0m,
                LoiNhuanUocTinh = 0m,
                PhanTramDoanhThu = 0
            }).ToList();
        }

        return new TopProductsReportDto
        {
            Items = grouped,
            TongDoanhThuTop = grouped.Sum(p => p.TongDoanhThu),
            TongSoLuongTop = grouped.Sum(p => p.SoLuongDaBan),
            TongDoanhThuToanPhan = totalRevenueAll,
            TuNgay = fromDate,
            DenNgay = toDate
        };
    }

    private static (DateTime FromDate, DateTime ToDate) ResolveDateRange(
        string? period, DateTime? customFrom, DateTime? customTo, DateTime now)
    {
        var p = (period ?? "thisMonth").ToLowerInvariant().Trim();

        if (p == "custom" && customFrom.HasValue && customTo.HasValue)
        {
            return (customFrom.Value.Date, customTo.Value.Date.AddDays(1).AddTicks(-1));
        }

        switch (p)
        {
            case "today":
                return (now.Date, now.Date.AddDays(1).AddTicks(-1));
            case "yesterday":
                var y = now.Date.AddDays(-1);
                return (y, y.AddDays(1).AddTicks(-1));
            case "7days":
                return (now.Date.AddDays(-6), now.Date.AddDays(1).AddTicks(-1));
            case "30days":
                return (now.Date.AddDays(-29), now.Date.AddDays(1).AddTicks(-1));
            case "lastmonth":
                var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonthEnd = firstOfThisMonth.AddTicks(-1);
                var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);
                return (lastMonthStart, lastMonthEnd);
            case "thismonth":
            default:
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
                return (startOfMonth, endOfMonth);
        }
    }
}
