using Souvenir.Domain.Entities;

namespace Souvenir.Application.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public int VaiTroId { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int SoLanDangNhapSai { get; set; }

    public static UserDto FromEntity(NguoiDung u) => new()
    {
        Id = u.Id,
        TenDangNhap = u.TenDangNhap,
        HoTen = u.HoTen,
        SoDienThoai = u.SoDienThoai?.Trim() ?? string.Empty,
        VaiTroId = u.VaiTroId,
        TenVaiTro = u.VaiTro?.TenVaiTro ?? string.Empty,
        TrangThai = u.TrangThai,
        SoLanDangNhapSai = u.SoLanDangNhapSai
    };
}

public class RoleDto
{
    public int Id { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class CreateUserRequest
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public int VaiTroId { get; set; }
}

public class UpdateUserRequest
{
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public int VaiTroId { get; set; }
}

public class ChangeUserStatusRequest
{
    public string? TrangThai { get; set; }
}

public class UserOperationResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static UserOperationResult<T> Success(T data, int statusCode = 200, string? message = null) =>
        new() { IsSuccess = true, StatusCode = statusCode, Data = data, Message = message };

    public static UserOperationResult<T> Failure(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}

public class UserOperationResult : UserOperationResult<object>
{
    public static UserOperationResult SuccessResult(int statusCode = 200, string? message = null) =>
        new() { IsSuccess = true, StatusCode = statusCode, Message = message };

    public static new UserOperationResult Failure(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
