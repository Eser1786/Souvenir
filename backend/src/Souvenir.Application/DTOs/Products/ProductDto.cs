namespace Souvenir.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string TenDanhMuc { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string? HinhAnh { get; set; }
    public string? TenDonVi { get; set; }
}
