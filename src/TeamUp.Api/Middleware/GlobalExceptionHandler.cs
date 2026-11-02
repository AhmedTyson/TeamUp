using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;

namespace TeamUp.Api.Middleware;

/// <summary>Maps unhandled exceptions and domain errors to RFC 7807 problem+json.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problems)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title, detail) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed", ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", ex.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict Detected", ex.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized Access", ex.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden Access", ex.Message),
            BadRequestException => (StatusCodes.Status400BadRequest, "Invalid Request", ex.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", ex.Message),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) =>
                (StatusCodes.Status409Conflict, "Conflict Detected", "A conflicting unique record already exists in the database."),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled server error for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        else
            logger.LogWarning("Handled exception ({Status}) for {Method} {Path}: {Message}", status, ctx.Request.Method, ctx.Request.Path, ex.Message);

        ctx.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path,
        };

        if (ex is ValidationException valEx && valEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = valEx.Errors;
        }

        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            Exception = ex,
            ProblemDetails = problemDetails,
        });
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
