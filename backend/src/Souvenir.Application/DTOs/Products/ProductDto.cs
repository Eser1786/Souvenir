using System.Text.Json.Serialization;

namespace Souvenir.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int DanhMucId { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
    public int DonViTinhId { get; set; }
    public string? TenDonVi { get; set; }
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string TrangThai { get; set; } = "KinhDoanh";

    [JsonPropertyName("imageUrl")]
    public string? HinhAnh { get; set; }
}

public class CreateProductRequest
{
    public string? MaSP { get; set; }
    public string TenSP { get; set; } = string.Empty;
    public int? DanhMucId { get; set; }
    public string? TenDanhMuc { get; set; }
    public int? DonViTinhId { get; set; }
    public string? TenDonVi { get; set; }
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; } = 0;
    public string? ImageUrl { get; set; }
}

public class UpdateProductRequest
{
    public string TenSP { get; set; } = string.Empty;
    public int? DanhMucId { get; set; }
    public string? TenDanhMuc { get; set; }
    public int? DonViTinhId { get; set; }
    public string? TenDonVi { get; set; }
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string? TrangThai { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ProductResult<T> Success(T data, string message = "") =>
        new() { IsSuccess = true, StatusCode = 200, Data = data, Message = message };

    public static ProductResult<T> Created(T data, string message = "Thêm mới sản phẩm thành công.") =>
        new() { IsSuccess = true, StatusCode = 201, Data = data, Message = message };

    public static ProductResult<T> Failed(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
