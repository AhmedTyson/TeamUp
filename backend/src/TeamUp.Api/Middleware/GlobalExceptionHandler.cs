using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Middleware;

/// <summary>
/// Catches unhandled and domain exceptions, returning a clean, human-readable BaseResponse error payload.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, message, errors) = ex switch
        {
            ValidationException valEx => (StatusCodes.Status400BadRequest, valEx.Message, (IReadOnlyList<ErrorDetail>?)valEx.Errors),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message, null),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.Message, null),
            UnauthorizedException unauth => (StatusCodes.Status401Unauthorized, unauth.Message, null),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, forbidden.Message, null),
            BadRequestException badReq => (StatusCodes.Status400BadRequest, badReq.Message, null),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message, null),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) =>
                (StatusCodes.Status409Conflict, "A conflicting unique record already exists in the database.", null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected server error occurred. Please try again later.", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled server error for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        else
            logger.LogWarning("Handled exception ({Status}) for {Method} {Path}: {Message}", status, ctx.Request.Method, ctx.Request.Path, ex.Message);

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";

        var response = BaseResponse.Fail(message, status, errors);
        await ctx.Response.WriteAsJsonAsync(response, ct);
        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
