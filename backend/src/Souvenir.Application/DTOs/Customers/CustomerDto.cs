namespace Souvenir.Application.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string TenKhachHang { get; set; } = string.Empty;
    public int DiemTichLuy { get; set; }
    public string HangThanhVien => DiemTichLuy switch
    {
        >= 500 => "Kim Cương",
        >= 200 => "Vàng",
        >= 50 => "Bạc",
        _ => "Đồng"
    };
}
