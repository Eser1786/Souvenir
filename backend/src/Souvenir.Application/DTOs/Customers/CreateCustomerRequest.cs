using System.ComponentModel.DataAnnotations;

namespace Souvenir.Application.DTOs.Customers;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khách hàng không được để trống")]
    [StringLength(100, ErrorMessage = "Tên khách hàng tối đa 100 ký tự")]
    public string TenKhachHang { get; set; } = string.Empty;
}
