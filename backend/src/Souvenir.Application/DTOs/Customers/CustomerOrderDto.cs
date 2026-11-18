namespace Souvenir.Application.DTOs.Customers;

public class CustomerOrderItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class CustomerOrderDto
{
    public int Id { get; set; }
    public string MaDonHang { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal SoTienGiam { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string TenHinhThucThanhToan { get; set; } = string.Empty;
    public string? MaGiamGia { get; set; }
    public string TenNhanVien { get; set; } = string.Empty;
    public int SoLuongSanPham { get; set; }
    public List<CustomerOrderItemDto> Items { get; set; } = new();
}
