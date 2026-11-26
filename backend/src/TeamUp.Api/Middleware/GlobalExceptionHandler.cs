using System.Buffers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Middleware;

/// <summary>
/// Catches unhandled and domain exceptions, returning a clean, human-readable BaseResponse error payload.
/// </summary>
public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled server error for {Method} {Path}")]
    private static partial void LogUnhandledServerError(ILogger logger, Exception ex, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handled exception ({Status}) for {Method} {Path}: {Message}")]
    private static partial void LogHandledException(ILogger logger, int status, string method, PathString path, string message);

    private static readonly SearchValues<string> UniqueConstraintMarkers = SearchValues.Create(
        ["UNIQUE KEY", "duplicate key", "UNIQUE constraint failed"],
        StringComparison.OrdinalIgnoreCase);

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, message, errors) = exception switch
        {
            ValidationException valEx => (StatusCodes.Status400BadRequest, valEx.Message, (IReadOnlyList<ErrorDetail>?)valEx.Errors),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message, null),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.Message, null),
            UnauthorizedException unauth => (StatusCodes.Status401Unauthorized, unauth.Message, null),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, forbidden.Message, null),
            BadRequestException badReq => (StatusCodes.Status400BadRequest, badReq.Message, null),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message, null),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "The record was modified concurrently by another request. Please refresh and try again.", null),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) =>
                (StatusCodes.Status409Conflict, "A conflicting unique record already exists in the database.", null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected server error occurred. Please try again later.", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            LogUnhandledServerError(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        else
            LogHandledException(logger, status, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        var response = BaseResponse.Fail(message, status, errors);
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.AsSpan().ContainsAny(UniqueConstraintMarkers);
    }
}
