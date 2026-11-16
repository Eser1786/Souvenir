namespace Souvenir.Application.DTOs.Auth;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public int? RemainingAttempts { get; set; }
    public LoginResponse? Data { get; set; }

    public static AuthResult Success(LoginResponse data) =>
        new() { IsSuccess = true, StatusCode = 200, Data = data };

    public static AuthResult Locked(string message) =>
        new() { IsSuccess = false, StatusCode = 403, IsLocked = true, Message = message };

    public static AuthResult Failed(string message, int statusCode = 400, int? remainingAttempts = null, bool isLocked = false) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message, RemainingAttempts = remainingAttempts, IsLocked = isLocked };
}
