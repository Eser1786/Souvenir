namespace Souvenir.Application.DTOs.Orders;

public class CreateDraftOrderItemRequest
{
    public int SanPhamId { get; set; }
    public int SoLuong { get; set; }
}

public class CreateDraftOrderRequest
{
    public List<CreateDraftOrderItemRequest> Items { get; set; } = new();
    public int? KhachHangId { get; set; }
    public string? MaGiamGia { get; set; }
    public int DiemSuDung { get; set; }
    public int? CaLamViecId { get; set; }
    public string? GhiChu { get; set; }
}

public class DraftOrderItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public int SoLuongTon { get; set; }
}

public class DraftOrderDto
{
    public int Id { get; set; }
    public string MaDonHang { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string TrangThai { get; set; } = "LuuNhap";
    public int NguoiDungId { get; set; }
    public string? TenNhanVien { get; set; }
    public int? KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public string? SoDienThoaiKhach { get; set; }
    public string? MaGiamGia { get; set; }
    public int DiemSuDung { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal SoTienGiam { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public int TongSoLuongMon { get; set; }
    public List<DraftOrderItemDto> Items { get; set; } = new();
}
