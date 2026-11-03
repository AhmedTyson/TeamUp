namespace TeamUp.Application.Common.Models;

public sealed record ErrorDetail(string Field, string Message);

/// <summary>
/// Unified, human-readable response envelope for all API endpoints (success and error).
/// </summary>
public class BaseResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public IReadOnlyList<ErrorDetail>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static BaseResponse Ok(string message = "Operation completed successfully.", int statusCode = 200) =>
        new() { Success = true, Message = message, StatusCode = statusCode };

    public static BaseResponse Fail(string message, int statusCode = 400, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors?.ToList() };
}

/// <summary>
/// Unified generic base response envelope wrapping payload data.
/// </summary>
public class BaseResponse<T> : BaseResponse
{
    public T? Data { get; init; }

    public static BaseResponse<T> Ok(T data, string message = "Operation completed successfully.", int statusCode = 200) =>
        new() { Success = true, Message = message, StatusCode = statusCode, Data = data };

    public new static BaseResponse<T> Fail(string message, int statusCode = 400, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Data = default, Errors = errors?.ToList() };
}

/// <summary>
/// Dedicated error response model matching BaseResponse.
/// </summary>
public class ErrorResponse : BaseResponse;
