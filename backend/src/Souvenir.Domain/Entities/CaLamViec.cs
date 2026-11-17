using Souvenir.Domain.Enums;

namespace Souvenir.Domain.Entities;

public class CaLamViec
{
    public int Id { get; set; }
    public int NguoiDungId { get; set; }
    public DateTime ThoiGianMoCa { get; set; } = DateTime.UtcNow;
    public DateTime? ThoiGianChotCa { get; set; }
    public decimal TienMatBanGiao { get; set; }
    public decimal? TienMatThucTe { get; set; }
    public string TrangThai { get; set; } = nameof(TrangThaiCaLamViec.DANG_MO);
    public string? LyDoChenhLech { get; set; }

    public ChamCong? ChamCong { get; set; }

    public void ChotCa(decimal tienMatThucTe, decimal tongDoanhThuTienMat)
    {
        if (TrangThai == nameof(TrangThaiCaLamViec.DA_CHOT))
            throw new InvalidOperationException("Ca làm việc đã chốt, không thể thao tác lại.");

        TienMatThucTe = tienMatThucTe;
        ThoiGianChotCa = DateTime.UtcNow;

        var tienLyThuyet = TienMatBanGiao + tongDoanhThuTienMat;
        TrangThai = (tienMatThucTe == tienLyThuyet)
            ? nameof(TrangThaiCaLamViec.DA_CHOT)
            : nameof(TrangThaiCaLamViec.CHO_XU_LY_CHENH_LECH);
    }

    public void GiaiTrinhChenhLech(string lyDo)
    {
        if (TrangThai != nameof(TrangThaiCaLamViec.CHO_XU_LY_CHENH_LECH))
            throw new InvalidOperationException("Ca không ở trạng thái chờ xử lý chênh lệch.");
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new ArgumentException("Vui lòng nhập lý do giải trình hợp lệ.");

        LyDoChenhLech = lyDo;
        TrangThai = nameof(TrangThaiCaLamViec.DA_CHOT);
    }
}