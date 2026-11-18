namespace Souvenir.Application.DTOs.Returns;

/// <summary>Một dòng sản phẩm của hóa đơn gốc, kèm số lượng còn được trả.</summary>
public class ReturnableItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? HinhAnh { get; set; }
    public int SoLuongMua { get; set; }
    public int SoLuongDaTra { get; set; }
    public int SoLuongConTra { get; set; }
    /// <summary>Đơn giá bán gốc trên hóa đơn.</summary>
    public decimal DonGia { get; set; }
    /// <summary>Đơn giá hoàn = đơn giá gốc đã phân bổ giảm giá (voucher/điểm) của đơn.</summary>
    public decimal DonGiaHoan { get; set; }
}

/// <summary>Hóa đơn gốc dùng cho màn hình tra cứu trả hàng.</summary>
public class ReturnableOrderDto
{
    public int Id { get; set; }
    public string MaDonHang { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? KhachHangId { get; set; }
    public string? TenKhachHang { get; set; }
    public string? SoDienThoaiKhach { get; set; }
    public string TenHinhThucThanhToan { get; set; } = string.Empty;
    public string? MaGiamGia { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public decimal TongDaHoan { get; set; }
    public int SoNgayChoPhepDoiTra { get; set; }
    public DateTime HanDoiTra { get; set; }
    public bool ConHanDoiTra { get; set; }
    /// <summary>true nếu còn hạn, đúng trạng thái và còn sản phẩm chưa trả.</summary>
    public bool CoTheTra { get; set; }
    public string? LyDoKhongTheTra { get; set; }
    public List<ReturnableItemDto> Items { get; set; } = new();
}

public class CreateReturnItemRequest
{
    public int SanPhamId { get; set; }
    public int SoLuongTra { get; set; }
    /// <summary>"NguyenVen" | "LoiNhaSanXuat" | "HuHongDoKhach" (bị từ chối).</summary>
    public string TinhTrangHang { get; set; } = "NguyenVen";
}

public class CreateReturnRequest
{
    /// <summary>Id đơn gốc. Có thể thay bằng MaDonHang.</summary>
    public int? DonHangId { get; set; }
    /// <summary>Mã đơn dạng DH261117-00012.</summary>
    public string? MaDonHang { get; set; }
    public string LyDoTra { get; set; } = string.Empty;
    public List<CreateReturnItemRequest> Items { get; set; } = new();
}

public class ReturnItemDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int SoLuongTra { get; set; }
    public decimal DonGiaHoan { get; set; }
    public decimal ThanhTien { get; set; }
    public string TinhTrangHang { get; set; } = string.Empty;
    public int? SoLuongTonMoi { get; set; }
}

public class ReturnDto
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int DonHangGocId { get; set; }
    public string MaDonHangGoc { get; set; } = string.Empty;
    public DateTime NgayTra { get; set; }
    public int NguoiDungId { get; set; }
    public string? TenNguoiLap { get; set; }
    public string LyDoTra { get; set; } = string.Empty;
    public decimal TongTienHoan { get; set; }
    /// <summary>Hoàn theo hình thức thanh toán của đơn gốc (tiền mặt / chuyển khoản).</summary>
    public string HinhThucHoan { get; set; } = string.Empty;
    public string TrangThaiDonGoc { get; set; } = string.Empty;
    public int DiemTichLuyBiTru { get; set; }
    public List<ReturnItemDto> Items { get; set; } = new();
}

public class ReturnOperationResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ReturnOperationResult<T> Success(T data, int statusCode = 200, string? message = null) =>
        new() { IsSuccess = true, StatusCode = statusCode, Data = data, Message = message };

    public static ReturnOperationResult<T> Failure(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
