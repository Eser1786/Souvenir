namespace Souvenir.Domain.Entities;

/// <summary>Chi tiết kiểm đếm từng sản phẩm trong phiếu kiểm kê (UC13).</summary>
public class ChiTietKiemKe
{
    public int PhieuKiemKeId { get; set; }
    public int SanPhamId { get; set; }
    public int SoTonSoSach { get; set; }
    public int SoTonThucTe { get; set; }
    public int SoLuongChenhLech { get; set; }
    public string? LyDoChenhLech { get; set; }

    public PhieuKiemKe? PhieuKiemKe { get; set; }
    public SanPham? SanPham { get; set; }

    public int TinhChenhLech()
    {
        SoLuongChenhLech = SoTonThucTe - SoTonSoSach;
        return SoLuongChenhLech;
    }
}
