namespace Souvenir.Application.DTOs.Customers;

public class CustomerResult<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static CustomerResult<T> Success(T data, string message = "") =>
        new() { IsSuccess = true, StatusCode = 200, Data = data, Message = message };

    public static CustomerResult<T> Created(T data, string message = "Thêm mới khách hàng thành công.") =>
        new() { IsSuccess = true, StatusCode = 201, Data = data, Message = message };

    public static CustomerResult<T> Failed(string message, int statusCode = 400) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message };
}
