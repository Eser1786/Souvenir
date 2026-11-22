namespace Souvenir.Application.DTOs.Auth;

public class LogoutResult
{
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = "Đăng xuất thành công.";

    public static LogoutResult Success(string message = "Đăng xuất thành công.") =>
        new() { IsSuccess = true, Message = message };
}
