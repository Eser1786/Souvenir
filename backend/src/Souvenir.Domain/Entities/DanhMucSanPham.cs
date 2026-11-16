namespace Souvenir.Domain.Entities;

public class DanhMucSanPham
{
    public int Id { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "HoatDong";

    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
