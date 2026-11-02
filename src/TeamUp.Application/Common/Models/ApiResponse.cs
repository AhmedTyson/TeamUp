namespace TeamUp.Application.Common.Models;

public sealed record ErrorDetail(string Field, string Message);

/// <summary>
/// Standardized JSON response envelope for API endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<ErrorDetail>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully.") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResult(string message, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors?.ToList() };
}

/// <summary>
/// Structured error payload matching RFC 7807 with field-level details.
/// </summary>
public class ErrorResponse
{
    public string Type { get; init; } = "https://tools.ietf.org/html/rfc9110";
    public string Title { get; init; } = "An error occurred.";
    public int Status { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }
    public IReadOnlyList<ErrorDetail> Errors { get; init; } = [];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
