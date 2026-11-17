namespace Souvenir.Domain.Entities;

/// <summary>
/// Đơn hàng bán tại quầy POS (UC04/UC07).
/// Các khóa ngoại tới phân hệ của người khác (ca làm việc, khách hàng, mã giảm giá)
/// được giữ ở dạng Id "lỏng" để không phụ thuộc vào entity chưa có.
/// </summary>
public class DonHang
{
    public int Id { get; set; }

    /// <summary>Ca làm việc đang mở (UC03 - Người 5). Bảng DB đang NOT NULL nên service sẽ bắt buộc có giá trị.</summary>
    public int? CaLamViecId { get; set; }

    /// <summary>Khách hàng thành viên (UC09 - Người 6). NULL = khách vãng lai.</summary>
    public int? KhachHangId { get; set; }

    public int NguoiDungId { get; set; }

    /// <summary>Mã giảm giá đã áp dụng (UC05 - Người 2). NULL = không dùng voucher.</summary>
    public int? MaGiamGiaId { get; set; }

    public int HinhThucThanhToanId { get; set; }
    public DateTime NgayTao { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public string TrangThai { get; set; } = TrangThaiDonHang.DangLapDon;

    public NguoiDung? NguoiDung { get; set; }
    public MaGiamGia? MaGiamGia { get; set; }
    public HinhThucThanhToan? HinhThucThanhToan { get; set; }
    public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    /// <summary>Tổng tiền hàng trước giảm giá.</summary>
    public decimal TinhTongTienHang() => ChiTietDonHangs.Sum(ct => ct.ThanhTien);
}

/// <summary>Các trạng thái theo sơ đồ trạng thái Đơn hàng (docs/state/state-don-hang.puml).</summary>
public static class TrangThaiDonHang
{
    public const string DangLapDon = "DangLapDon";
    public const string LuuNhap = "LuuNhap";
    public const string ChoThanhToan = "ChoThanhToan";
    public const string DaThanhToan = "DaThanhToan";
    public const string DaHuy = "DaHuy";
    public const string DaTraHang = "DaTraHang";
}
