using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Shifts.DTOs;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;
using Souvenir.Domain.Enums;
using Souvenir.Infrastructure.Persistence.Configurations;
using Souvenir.Infrastructure.Data;

namespace Souvenir.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly AppDbContext _db;

    public ShiftService(AppDbContext db) => _db = db;

    public async Task<ShiftResponse?> GetCurrentShiftAsync(int userId)
    {
        var shift = await _db.CaLamViecs
            .Include(x => x.ChamCong)
            .Where(x => x.NguoiDungId == userId && x.TrangThai != nameof(TrangThaiCaLamViec.DA_CHOT))
            .OrderByDescending(x => x.ThoiGianMoCa)
            .FirstOrDefaultAsync();

        return shift == null ? null : await MapToDtoAsync(shift);
    }

    public async Task<ShiftResponse> OpenShiftAsync(int userId, OpenShiftRequest request)
    {
        var existing = await _db.CaLamViecs
            .AnyAsync(x => x.NguoiDungId == userId && x.TrangThai != nameof(TrangThaiCaLamViec.DA_CHOT));

        if (existing)
            throw new InvalidOperationException("Bạn đang có một ca làm việc chưa chốt.");

        var shift = new CaLamViec
        {
            NguoiDungId = userId,
            TienMatBanGiao = request.TienMatBanGiao,
            ThoiGianMoCa = DateTime.UtcNow,
            TrangThai = nameof(TrangThaiCaLamViec.DANG_MO)
        };

        _db.CaLamViecs.Add(shift);
        await _db.SaveChangesAsync();

        // Tự động tạo bản ghi chấm công Check-in gắn với ca vừa mở
        var attendance = new ChamCong
        {
            NguoiDungId = userId,
            CaLamViecId = shift.Id,
            GioCheckIn = DateTime.UtcNow
        };
        _db.ChamCongs.Add(attendance);
        await _db.SaveChangesAsync();

        shift.ChamCong = attendance;
        return await MapToDtoAsync(shift);
    }

    public async Task<ShiftResponse> CloseShiftAsync(int userId, int shiftId, CloseShiftRequest request)
    {
        var shift = await _db.CaLamViecs
            .Include(x => x.ChamCong)
            .FirstOrDefaultAsync(x => x.Id == shiftId && x.NguoiDungId == userId)
            ?? throw new KeyNotFoundException("Không tìm thấy ca làm việc.");

        var cashRevenue = await CalculateCashRevenueAsync(shift.Id);
        shift.ChotCa(request.TienMatThucTe, cashRevenue);

        if (shift.ChamCong != null && shift.ChamCong.GioCheckOut == null)
        {
            shift.ChamCong.GioCheckOut = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return await MapToDtoAsync(shift);
    }

    public async Task<ShiftResponse> ExplainDiscrepancyAsync(int shiftId, ExplainShiftRequest request)
    {
        var shift = await _db.CaLamViecs
            .Include(x => x.ChamCong)
            .FirstOrDefaultAsync(x => x.Id == shiftId)
            ?? throw new KeyNotFoundException("Không tìm thấy ca làm việc.");

        shift.GiaiTrinhChenhLech(request.LyDoChenhLech);
        await _db.SaveChangesAsync();
        return await MapToDtoAsync(shift);
    }

    public async Task<AttendanceResponse> CheckInAsync(int userId)
    {
        var shift = await _db.CaLamViecs
            .Include(x => x.ChamCong)
            .FirstOrDefaultAsync(x => x.NguoiDungId == userId && x.TrangThai == nameof(TrangThaiCaLamViec.DANG_MO))
            ?? throw new InvalidOperationException("Vui lòng mở ca làm việc trước khi chấm công.");

        if (shift.ChamCong != null)
            return new AttendanceResponse(shift.ChamCong.Id, shift.Id, shift.ChamCong.GioCheckIn, shift.ChamCong.GioCheckOut);

        var cc = new ChamCong { NguoiDungId = userId, CaLamViecId = shift.Id, GioCheckIn = DateTime.UtcNow };
        _db.ChamCongs.Add(cc);
        await _db.SaveChangesAsync();
        return new AttendanceResponse(cc.Id, shift.Id, cc.GioCheckIn, cc.GioCheckOut);
    }

    public async Task<AttendanceResponse> CheckOutAsync(int userId)
    {
        var cc = await _db.ChamCongs
            .Where(x => x.NguoiDungId == userId && x.GioCheckOut == null)
            .OrderByDescending(x => x.GioCheckIn)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Bạn chưa check-in hoặc đã check-out rồi.");

        cc.GioCheckOut = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new AttendanceResponse(cc.Id, cc.CaLamViecId, cc.GioCheckIn, cc.GioCheckOut);
    }

    private async Task<decimal> CalculateCashRevenueAsync(int shiftId)
    {
        // Tính tổng tiền mặt từ bảng don_hang (hinhThucThanhToanId = 1 là Tiền mặt)
        // Dùng raw SQL an toàn khi Người 3 chưa merge Entity DonHang
        var result = await _db.Database
            .SqlQueryRaw<decimal>(
                "SELECT COALESCE(SUM(tongtienthanhtoan), 0) AS \"Value\" FROM don_hang WHERE calamviecid = {0} AND hinhthucthanhtoanid = 1 AND trangthai = 'DA_THANH_TOAN'",
                shiftId)
            .FirstOrDefaultAsync();
        return result;
    }

    private async Task<ShiftResponse> MapToDtoAsync(CaLamViec s)
    {
        var cashRevenue = await CalculateCashRevenueAsync(s.Id);
        var chenhLech = s.TienMatThucTe.HasValue
            ? s.TienMatThucTe.Value - (s.TienMatBanGiao + cashRevenue)
            : 0;

        var ccDto = s.ChamCong == null
            ? null
            : new AttendanceResponse(s.ChamCong.Id, s.Id, s.ChamCong.GioCheckIn, s.ChamCong.GioCheckOut);

        return new ShiftResponse(
            s.Id, s.NguoiDungId, s.ThoiGianMoCa, s.ThoiGianChotCa,
            s.TienMatBanGiao, s.TienMatThucTe, cashRevenue, chenhLech,
            s.TrangThai, s.LyDoChenhLech, ccDto
        );
    }
}