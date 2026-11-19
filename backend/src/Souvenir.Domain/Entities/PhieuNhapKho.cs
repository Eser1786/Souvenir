namespace Souvenir.Domain.Entities;

public class PhieuNhapKho
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int NhaCungCapId { get; set; }
    public int NguoiDungId { get; set; }
    public decimal TongTienNhap { get; set; }
    public DateTime NgayNhap { get; set; } = DateTime.UtcNow;

    public NhaCungCap? NhaCungCap { get; set; }
    public NguoiDung? NguoiDung { get; set; }
    public ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();
}
