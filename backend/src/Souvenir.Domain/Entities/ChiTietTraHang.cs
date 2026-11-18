namespace Souvenir.Domain.Entities;

/// <summary>Dòng sản phẩm trên phiếu trả. Khóa chính kép (PhieuTraHangId, SanPhamId).</summary>
public class ChiTietTraHang
{
    public int PhieuTraHangId { get; set; }
    public int SanPhamId { get; set; }
    public int SoLuongTra { get; set; }
    public decimal DonGiaHoan { get; set; }
    public string TinhTrangHang { get; set; } = TinhTrangHangTra.NguyenVen;

    public PhieuTraHang? PhieuTraHang { get; set; }
    public SanPham? SanPham { get; set; }
}
