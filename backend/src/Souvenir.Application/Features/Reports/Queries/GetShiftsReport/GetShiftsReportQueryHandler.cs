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

namespace Souvenir.Application.Features.Reports.Queries.GetShiftsReport;

public class GetShiftsReportQueryHandler : IRequestHandler<GetShiftsReportQuery, ShiftsReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetShiftsReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShiftsReportDto> Handle(GetShiftsReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var (fromDate, toDate) = ResolveDateRange(request.Period, request.FromDate, request.ToDate, now);

        var fromUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Local).ToUniversalTime();
        var toUtc = DateTime.SpecifyKind(toDate, DateTimeKind.Local).ToUniversalTime();

        // Load shifts in time range
        var shifts = await _context.CaLamViecs
            .AsNoTracking()
            .Where(s => s.ThoiGianMoCa >= fromUtc && s.ThoiGianMoCa <= toUtc)
            .OrderByDescending(s => s.ThoiGianMoCa)
            .ToListAsync(cancellationToken);

        // Preload users for fast name lookup
        var userIds = shifts.Select(s => s.NguoiDungId).Distinct().ToList();
        var usersMap = await _context.NguoiDungs
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.HoTen ?? u.TenDangNhap, cancellationToken);

        // Preload orders associated with these shifts
        var shiftIds = shifts.Select(s => s.Id).ToList();
        var shiftOrders = await _context.DonHangs
            .AsNoTracking()
            .Where(d => d.CaLamViecId.HasValue && shiftIds.Contains(d.CaLamViecId.Value))
            .Where(d => d.TrangThai == TrangThaiDonHang.DaThanhToan || d.TrangThai == "DA_THANH_TOAN")
            .ToListAsync(cancellationToken);

        var items = shifts.Select(s =>
        {
            var orders = shiftOrders.Where(o => o.CaLamViecId == s.Id).ToList();
            var cashRevenue = orders.Where(o => o.HinhThucThanhToanId == 1).Sum(o => o.TongTienThanhToan);
            var transferRevenue = orders.Where(o => o.HinhThucThanhToanId != 1).Sum(o => o.TongTienThanhToan);
            var totalRevenue = cashRevenue + transferRevenue;

            var chenhLech = s.TienMatThucTe.HasValue
                ? s.TienMatThucTe.Value - (s.TienMatBanGiao + cashRevenue)
                : 0m;

            return new ShiftReportItemDto
            {
                CaLamViecId = s.Id,
                NhanVienId = s.NguoiDungId,
                TenNhanVien = usersMap.TryGetValue(s.NguoiDungId, out var name) ? name : $"Nhân viên #{s.NguoiDungId}",
                ThoiGianMoCa = s.ThoiGianMoCa,
                ThoiGianChotCa = s.ThoiGianChotCa,
                TienMatBanGiao = s.TienMatBanGiao,
                TienMatThucTe = s.TienMatThucTe,
                TongDoanhThuTienMat = cashRevenue,
                TongDoanhThuChuyenKhoan = transferRevenue,
                TongDoanhThuCa = totalRevenue,
                ChenhLech = chenhLech,
                TrangThai = s.TrangThai,
                LyDoChenhLech = s.LyDoChenhLech,
                SoDonHang = orders.Count
            };
        }).ToList();

        return new ShiftsReportDto
        {
            Items = items,
            TongDoanhThuCacCa = items.Sum(i => i.TongDoanhThuCa),
            TongTienMatCacCa = items.Sum(i => i.TongDoanhThuTienMat),
            TongChuyenKhoanCacCa = items.Sum(i => i.TongDoanhThuChuyenKhoan),
            TongSoCa = items.Count,
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
