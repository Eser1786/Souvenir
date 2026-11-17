namespace Souvenir.Domain.Entities;

/// <summary>Dòng sản phẩm của đơn hàng. Khóa chính kép (DonHangId, SanPhamId).</summary>
public class ChiTietDonHang
{
    public int DonHangId { get; set; }
    public int SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public DonHang? DonHang { get; set; }
    public SanPham? SanPham { get; set; }
}
