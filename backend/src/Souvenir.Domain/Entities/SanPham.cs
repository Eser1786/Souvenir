namespace Souvenir.Domain.Entities;

public class SanPham
{
    public int Id { get; set; }
    public int DanhMucId { get; set; }
    public int DonViTinhId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string? HinhAnh { get; set; }
    public string TenSP { get; set; } = string.Empty;
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string TrangThai { get; set; } = "KinhDoanh";

    public DanhMucSanPham? DanhMuc { get; set; }
    public DonViTinh? DonViTinh { get; set; }
}
