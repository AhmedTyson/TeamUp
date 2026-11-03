namespace TeamUp.Application.Common.Models;

/// <summary>
/// Alias for BaseResponse supporting legacy conventions.
/// </summary>
public class ApiResponse<T> : BaseResponse<T>
{
    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully.") =>
        new() { Success = true, Message = message, StatusCode = 200, Data = data };

    public static ApiResponse<T> FailureResult(string message, IEnumerable<ErrorDetail>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = 400, Errors = errors?.ToList() };
}
