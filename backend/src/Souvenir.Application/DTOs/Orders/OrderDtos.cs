namespace Souvenir.Application.DTOs.Orders;

public class CreateOrderItemRequest
{
    public int SanPhamId { get; set; }
    public int SoLuong { get; set; }
}

public class CreateOrderRequest
{
    /// <summary>Danh sách sản phẩm trong giỏ.</summary>
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    /// <summary>"TienMat" hoặc "ChuyenKhoan" (QR). Bỏ qua nếu đã gửi HinhThucThanhToanId.</summary>
    public string? PhuongThuc { get; set; }

    /// <summary>Id trong bảng hinh_thuc_thanh_toan (tùy chọn, ưu tiên hơn PhuongThuc).</summary>
    public int? HinhThucThanhToanId { get; set; }

    /// <summary>Số tiền khách đưa (bắt buộc khi thanh toán tiền mặt).</summary>
    public decimal? TienKhachDua { get; set; }

    /// <summary>Ca làm việc. Để trống thì hệ thống tự lấy ca đang mở của nhân viên.</summary>
    public int? CaLamViecId { get; set; }

    /// <summary>Khách hàng thành viên. Để trống = khách vãng lai.</summary>
    public int? KhachHangId { get; set; }

    /// <summary>Mã voucher (ví dụ QUATANG10). Server sẽ kiểm tra lại hiệu lực.</summary>
    public string? MaGiamGia { get; set; }
}

public class OrderItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public int SoLuongTonConLai { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string MaDonHang { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int NguoiDungId { get; set; }
    public int? CaLamViecId { get; set; }
    public int? KhachHangId { get; set; }
    public int HinhThucThanhToanId { get; set; }
    public string TenHinhThucThanhToan { get; set; } = string.Empty;
    public string? MaGiamGia { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal SoTienGiam { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public decimal? TienKhachDua { get; set; }
    public decimal? TienThoi { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class PaymentMethodDto
{
    public int Id { get; set; }
    public string TenHinhThuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

public class OrderOperationResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static OrderOperationResult<T> Success(T data, int statusCode = 200, string? message = null) =>
        new() { IsSuccess = true, StatusCode = statusCode, Data = data, Message = message };

    public static OrderOperationResult<T> Failure(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
