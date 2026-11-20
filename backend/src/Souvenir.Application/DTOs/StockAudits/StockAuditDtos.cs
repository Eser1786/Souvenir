namespace Souvenir.Application.DTOs.StockAudits;

public class StockAuditItemRequest
{
    public int SanPhamId { get; set; }
    public int SoTonThucTe { get; set; }
    public string? LyDoChenhLech { get; set; }
}

public class SaveStockAuditRequest
{
    public int? Id { get; set; }
    public List<StockAuditItemRequest> Items { get; set; } = new();
}

public class StockAuditDetailDto
{
    public int SanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string TenDanhMuc { get; set; } = string.Empty;
    public string? TenDonVi { get; set; }
    public int SoTonSoSach { get; set; }
    public int SoTonThucTe { get; set; }
    public int SoLuongChenhLech { get; set; }
    public string? LyDoChenhLech { get; set; }
}

public class StockAuditDto
{
    public int Id { get; set; }
    public string MaPhieu { get; set; } = string.Empty;
    public int NguoiDungId { get; set; }
    public string TenNguoiKiemKe { get; set; } = string.Empty;
    public DateTime NgayKiemKe { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int TongSanPham { get; set; }
    public int SoSanPhamChenhLech { get; set; }
    public int TongChenhLechThua { get; set; }
    public int TongChenhLechThieu { get; set; }
    public List<StockAuditDetailDto> Items { get; set; } = new();
}

public class StockAuditResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static StockAuditResult<T> Success(T data, string message = "") =>
        new() { IsSuccess = true, StatusCode = 200, Data = data, Message = message };

    public static StockAuditResult<T> Created(T data, string message = "Lưu phiếu kiểm kê thành công.") =>
        new() { IsSuccess = true, StatusCode = 201, Data = data, Message = message };

    public static StockAuditResult<T> Failed(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
