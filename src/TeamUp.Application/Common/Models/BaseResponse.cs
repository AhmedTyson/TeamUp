namespace TeamUp.Application.Common.Models;

public static class HttpStatus
{
    public const int Ok = 200;
    public const int Created = 201;
    public const int NoContent = 204;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
    public const int TooManyRequests = 429;
    public const int InternalServerError = 500;
}

public sealed record ErrorDetail(string Field, string Message);

/// <summary>
/// Canonical industry-standard API response envelope for all endpoints.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public IReadOnlyList<ErrorDetail>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok(string message = "Operation completed successfully.", int statusCode = HttpStatus.Ok) =>
        new() { Success = true, Message = message, StatusCode = statusCode };

    public static ApiResponse Created(string message = "Resource created successfully.") =>
        new() { Success = true, Message = message, StatusCode = HttpStatus.Created };

    public static ApiResponse Fail(string message, int statusCode = HttpStatus.BadRequest, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors?.ToList() };
}

/// <summary>
/// Canonical generic API response envelope wrapping typed payload data.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully.", int statusCode = HttpStatus.Ok) =>
        new() { Success = true, Message = message, StatusCode = statusCode, Data = data };

    public static ApiResponse<T> Created(T data, string message = "Resource created successfully.") =>
        new() { Success = true, Message = message, StatusCode = HttpStatus.Created, Data = data };

    public new static ApiResponse<T> Fail(string message, int statusCode = HttpStatus.BadRequest, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Data = default, Errors = errors?.ToList() };
}

/// <summary>
/// Backward-compatible alias matching BaseResponse conventions.
/// </summary>
public class BaseResponse : ApiResponse;

/// <summary>
/// Backward-compatible generic alias matching BaseResponse&lt;T&gt; conventions.
/// </summary>
public class BaseResponse<T> : ApiResponse<T>;

/// <summary>
/// Dedicated error response model matching ApiResponse envelope.
/// </summary>
public class ErrorResponse : ApiResponse;
