namespace TeamUp.Application.Common.Models;

/// <summary>
/// Unified base response envelope for parameterless or message-only responses.
/// </summary>
public class BaseResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static BaseResponse Ok(string message = "Operation completed successfully.", int statusCode = 200) =>
        new() { Success = true, Message = message, StatusCode = statusCode };

    public static BaseResponse Fail(string message, int statusCode = 400) =>
        new() { Success = false, Message = message, StatusCode = statusCode };
}

/// <summary>
/// Unified generic base response envelope wrapping payload data.
/// </summary>
public class BaseResponse<T> : BaseResponse
{
    public T? Data { get; init; }

    public static BaseResponse<T> Ok(T data, string message = "Operation completed successfully.", int statusCode = 200) =>
        new() { Success = true, Message = message, StatusCode = statusCode, Data = data };

    public new static BaseResponse<T> Fail(string message, int statusCode = 400) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Data = default };
}
