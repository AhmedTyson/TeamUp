using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Exceptions;

namespace TeamUp.Api.Middleware;

/// <summary>Maps unhandled exceptions and domain errors to RFC 7807 problem+json.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problems)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict detected."),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized access."),
            BadRequestException => (StatusCodes.Status400BadRequest, "Invalid request."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled server error for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        else
            logger.LogWarning("Handled domain exception {Status} for {Method} {Path}: {Message}", status, ctx.Request.Method, ctx.Request.Path, ex.Message);

        ctx.Response.StatusCode = status;

        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            Exception = ex,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status != StatusCodes.Status500InternalServerError ? ex.Message : null,
                Instance = ctx.Request.Path,
            },
        });
    }
}
