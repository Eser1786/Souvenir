namespace Souvenir.Domain.Entities;

public class NguoiDung
{
    public int Id { get; set; }
    public int VaiTroId { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "HoatDong";
    public int SoLanDangNhapSai { get; set; } = 0;
    public DateTime? KhoaDenThoiDiem { get; set; }

    public VaiTro? VaiTro { get; set; }
}
