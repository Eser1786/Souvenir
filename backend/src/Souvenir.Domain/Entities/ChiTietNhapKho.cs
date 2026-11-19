namespace Souvenir.Domain.Entities;

public class ChiTietNhapKho
{
    public int PhieuNhapKhoId { get; set; }
    public int SanPhamId { get; set; }
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }

    public PhieuNhapKho? PhieuNhapKho { get; set; }
    public SanPham? SanPham { get; set; }
}
