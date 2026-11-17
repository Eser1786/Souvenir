namespace Souvenir.Domain.Entities;

public class KhachHang
{
    public int Id { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public int DiemTichLuy { get; set; } = 0;
}
