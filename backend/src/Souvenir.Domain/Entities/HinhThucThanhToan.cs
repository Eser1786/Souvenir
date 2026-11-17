namespace Souvenir.Domain.Entities;

/// <summary>Danh mục hình thức thanh toán (Tiền mặt, Chuyển khoản QR, ...).</summary>
public class HinhThucThanhToan
{
    public int Id { get; set; }
    public string TenHinhThuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "HoatDong";
}
