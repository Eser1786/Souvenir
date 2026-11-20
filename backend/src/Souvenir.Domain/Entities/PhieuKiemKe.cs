namespace Souvenir.Domain.Entities;

/// <summary>Phiếu kiểm kê kho (UC13).</summary>
public class PhieuKiemKe
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int NguoiDungId { get; set; }
    public DateTime NgayKiemKe { get; set; }
    public string TrangThai { get; set; } = TrangThaiPhieuKiemKe.DangKiemKe;

    public NguoiDung? NguoiDung { get; set; }
    public ICollection<ChiTietKiemKe> ChiTietKiemKes { get; set; } = new List<ChiTietKiemKe>();
}

public static class TrangThaiPhieuKiemKe
{
    public const string DangKiemKe = "DangKiemKe";
    public const string DaCanBang = "DaCanBang";
}
