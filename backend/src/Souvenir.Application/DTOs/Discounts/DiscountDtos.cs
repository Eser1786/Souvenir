namespace Souvenir.Application.DTOs.Discounts;

public class CheckDiscountRequest
{
    public string MaCode { get; set; } = string.Empty;
    public decimal TongTienHang { get; set; } = 0;
}

public class CheckDiscountResponse
{
    public bool HopLe { get; set; }
    public string ThongBao { get; set; } = string.Empty;
    public string? MaCode { get; set; }
    public string? TenVoucher { get; set; }
    public string? LoaiGiamGia { get; set; } // "PhanTram" hoặc "SoTien"
    public decimal GiaTriGiam { get; set; }
    public decimal SoTienGiam { get; set; }
    public decimal ThanhTienSauGiam { get; set; }
    public DateTime? NgayKetThuc { get; set; }
}
