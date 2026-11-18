namespace Souvenir.Domain.Entities;

/// <summary>Phiếu trả hàng / hoàn tiền (UC08). Một đơn gốc có thể có nhiều phiếu trả (trả từng phần).</summary>
public class PhieuTraHang
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int DonHangGocId { get; set; }
    public int NguoiDungId { get; set; }
    public decimal TongTienHoan { get; set; }
    public string LyDoTra { get; set; } = string.Empty;
    public DateTime NgayTra { get; set; }

    public DonHang? DonHangGoc { get; set; }
    public NguoiDung? NguoiDung { get; set; }
    public ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; } = new List<ChiTietTraHang>();
}

/// <summary>Tình trạng hàng khi khách mang trả.</summary>
public static class TinhTrangHangTra
{
    public const string NguyenVen = "NguyenVen";
    public const string LoiNhaSanXuat = "LoiNhaSanXuat";
    /// <summary>Hư hỏng do khách → từ chối đổi trả (luồng 3a của UC08).</summary>
    public const string HuHongDoKhach = "HuHongDoKhach";

    public static readonly string[] HopLe = { NguyenVen, LoiNhaSanXuat, HuHongDoKhach };
}
