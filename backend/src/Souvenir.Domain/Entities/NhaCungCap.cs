namespace Souvenir.Domain.Entities;

public class NhaCungCap
{
    public int Id { get; set; }
    public string MaNCC { get; set; } = string.Empty;
    public string TenNCC { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? DiaChi { get; set; }

    public ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; } = new List<PhieuNhapKho>();
}
