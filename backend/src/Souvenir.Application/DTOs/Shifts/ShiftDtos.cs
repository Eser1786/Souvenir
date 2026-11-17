namespace Souvenir.Application.Shifts.DTOs;

public record OpenShiftRequest(decimal TienMatBanGiao);
public record CloseShiftRequest(decimal TienMatThucTe);
public record ExplainShiftRequest(string LyDoChenhLech);

public record ShiftResponse(
    int Id,
    int NguoiDungId,
    DateTime ThoiGianMoCa,
    DateTime? ThoiGianChotCa,
    decimal TienMatBanGiao,
    decimal? TienMatThucTe,
    decimal TongDoanhThuTienMat,
    decimal ChenhLech,
    string TrangThai,
    string? LyDoChenhLech,
    AttendanceResponse? ChamCong
);

public record AttendanceResponse(
    int Id,
    int CaLamViecId,
    DateTime GioCheckIn,
    DateTime? GioCheckOut
);