// Date: 2026-11-26
// Purpose: Infrastructure DI registration wiring EF Core 11 (with Development SQLite fallback when SQL Server is offline), TimeProvider, JWT signing, SMTP/dev email transport, templates, and domain event dispatching.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Admin;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.EventDispatching;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Reports;
using TeamUp.Infrastructure.Services;

namespace TeamUp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        #region Options & Validation

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.Configure<EmailOptions>(config.GetSection(EmailOptions.SectionName));
        services.AddOptions<AttendanceOptions>()
            .Bind(config.GetSection(AttendanceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<GamificationOptions>()
            .Bind(config.GetSection(GamificationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<NotificationOptions>()
            .Bind(config.GetSection(NotificationOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => o.DefaultInboxTake <= o.MaxInboxTake, "Notifications:DefaultInboxTake must be <= MaxInboxTake.")
            .ValidateOnStart();
        services.AddOptions<AdminOptions>()
            .Bind(config.GetSection(AdminOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<RateLimitingOptions>()
            .Bind(config.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        #endregion

        #region Infrastructure Services

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher<Student>, PasswordHasher<Student>>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IAdminReportService, AdminReportPdfService>();

        var emailProvider = config["Email:Provider"] ?? "Smtp";
        if (emailProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<InMemoryEmailSender>();
            services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<InMemoryEmailSender>());
        }
        else
        {
            services.AddTransient<SmtpEmailSender>();
            services.AddTransient<IEmailSender>(sp => sp.GetRequiredService<SmtpEmailSender>());
        }

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        #endregion

        #region Persistence & Interceptors

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        var cs = config.GetConnectionString("TeamUpDb")
            ?? config.GetConnectionString("DefaultConnection");
        var provider = config["DatabaseProvider"] ?? "SqlServer";
        var allowSqliteFallback = config.GetValue("UseSqliteFallbackIfOffline", false);

        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            // When ConnectionString is empty (e.g., TeamUpApiFactory integration tests),
            // let the test host register its own shared in-memory SqliteConnection.
            if (!string.IsNullOrWhiteSpace(cs))
            {
                AddSqliteAppDbContext(services, cs);
            }
        }
        else if (!string.IsNullOrWhiteSpace(cs))
        {
            if (allowSqliteFallback && !IsSqlServerReachable(cs))
            {
                var sqliteCs = config.GetConnectionString("SqliteFallback") ?? "Data Source=TeamUp.dev.db";
                AddSqliteAppDbContext(services, sqliteCs);
            }
            else
            {
                services.AddDbContext<AppDbContext>((sp, o) => o
                    .UseSqlServer(cs, sql => sql.EnableRetryOnFailure().UseCompatibilityLevel(160))
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
            }
        }
        else if (allowSqliteFallback)
        {
            var sqliteCs = config.GetConnectionString("SqliteFallback") ?? "Data Source=TeamUp.dev.db";
            AddSqliteAppDbContext(services, sqliteCs);
        }

        #endregion

        return services;
    }

    private static void AddSqliteAppDbContext(IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<AppDbContext>((sp, o) => o
            .UseSqlite(sqliteConnectionString)
            .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
    }

    private static bool IsSqlServerReachable(string connectionString)
    {
        try
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 2,
            };
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString);
            conn.Open();
            return true;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Console.Error.WriteLine($"[TeamUp] SQL Server unreachable ({ex.Message}); falling back to local SQLite.");
            return false;
        }
    }
}
