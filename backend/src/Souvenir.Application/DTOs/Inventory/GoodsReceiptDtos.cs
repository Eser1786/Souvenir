namespace Souvenir.Application.DTOs.Inventory;

public class GoodsReceiptItemRequest
{
    public int? SanPhamId { get; set; }
    public string? MaSP { get; set; }
    public string? TenSP { get; set; }
    public int? DanhMucId { get; set; }
    public string? TenDanhMuc { get; set; }
    public int? DonViTinhId { get; set; }
    public string? TenDonVi { get; set; }
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
}

public class CreateGoodsReceiptRequest
{
    public int NhaCungCapId { get; set; }
    public List<GoodsReceiptItemRequest> ChiTiet { get; set; } = new();
}

public class GoodsReceiptDetailDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? TenDonVi { get; set; }
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
    public int SoLuongTonMoi { get; set; }
}

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int NhaCungCapId { get; set; }
    public string TenNCC { get; set; } = string.Empty;
    public int NguoiDungId { get; set; }
    public string TenNguoiNhap { get; set; } = string.Empty;
    public decimal TongTienNhap { get; set; }
    public DateTime NgayNhap { get; set; }
    public List<GoodsReceiptDetailDto> ChiTiet { get; set; } = new();
}

public class SupplierDto
{
    public int Id { get; set; }
    public string MaNCC { get; set; } = string.Empty;
    public string TenNCC { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
}

public class GoodsReceiptResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static GoodsReceiptResult<T> Success(T data, string message = "") =>
        new() { IsSuccess = true, StatusCode = 200, Data = data, Message = message };

    public static GoodsReceiptResult<T> Created(T data, string message = "Tạo phiếu nhập kho thành công.") =>
        new() { IsSuccess = true, StatusCode = 201, Data = data, Message = message };

    public static GoodsReceiptResult<T> Failed(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
