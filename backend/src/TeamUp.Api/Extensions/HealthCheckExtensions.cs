// Date: 2026-11-26
// Purpose: Health check registration and endpoints (/health, /health/live, /health/ready) with safe DB connectivity probing.

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Api.Extensions;

public static partial class HealthCheckExtensions
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Database readiness health check failed.")]
    private static partial void LogHealthCheckFailed(ILogger logger, Exception ex);
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTeamUpHealthChecks()
        {
            services.AddHealthChecks()
                .AddCheck<DatabaseReadinessHealthCheck>("database", tags: ["ready"]);
            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapTeamUpHealthEndpoints()
        {
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = report.Status.ToString(),
                    });
                }
            });

            var readyOptions = new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    var dbEntry = report.Entries.TryGetValue("database", out var entry)
                        ? entry.Description ?? entry.Status.ToString()
                        : report.Status.ToString();
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = report.Status.ToString(),
                        database = dbEntry
                    });
                }
            };

            app.MapHealthChecks("/health/ready", readyOptions);

            // Backward-compatible aggregate health probe preserved for existing monitors and E2E scripts.
            app.MapHealthChecks("/health", readyOptions);

            return app;
        }
    }

    private sealed class DatabaseReadinessHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseReadinessHealthCheck> logger) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db is null)
                    return HealthCheckResult.Healthy("NotConfigured");

                var canConnect = await db.Database.CanConnectAsync(cancellationToken);
                return canConnect
                    ? HealthCheckResult.Healthy("Connected")
                    : HealthCheckResult.Unhealthy("Disconnected");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogHealthCheckFailed(logger, ex);
                return HealthCheckResult.Unhealthy("Disconnected", ex);
            }
        }
    }
}
