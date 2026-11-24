// Date: 2026-11-22
// Purpose: Infrastructure DI registration wiring EF Core (with Development SQLite fallback when SQL Server is offline), JWT signing, SMTP/dev email transport, templates, and domain event dispatching.

using System.Net.Sockets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.EventDispatching;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;

namespace TeamUp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.Configure<EmailOptions>(config.GetSection(EmailOptions.SectionName));
        services.Configure<AttendanceOptions>(config.GetSection(AttendanceOptions.SectionName));
        services.Configure<AdminOptions>(config.GetSection(AdminOptions.SectionName));
        services.Configure<RateLimitingOptions>(config.GetSection(RateLimitingOptions.SectionName));

        services.AddSingleton<IPasswordHasher<Student>, PasswordHasher<Student>>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();

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
                services.AddDbContext<AppDbContext>((sp, o) => o
                    .UseSqlite(cs)
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
            }
        }
        else if (!string.IsNullOrWhiteSpace(cs))
        {
            if (allowSqliteFallback && !IsSqlServerReachable(cs))
            {
                var sqliteCs = config.GetConnectionString("SqliteFallback") ?? "Data Source=TeamUp.dev.db";
                services.AddDbContext<AppDbContext>((sp, o) => o
                    .UseSqlite(sqliteCs)
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
            }
            else
            {
                services.AddDbContext<AppDbContext>((sp, o) => o
                    .UseSqlServer(cs, sql => sql.EnableRetryOnFailure())
                    .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
            }
        }
        else if (allowSqliteFallback)
        {
            var sqliteCs = config.GetConnectionString("SqliteFallback") ?? "Data Source=TeamUp.dev.db";
            services.AddDbContext<AppDbContext>((sp, o) => o
                .UseSqlite(sqliteCs)
                .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
        }

        return services;
    }

    private static bool IsSqlServerReachable(string connectionString)
    {
        try
        {
            var segments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string? serverSegment = null;
            foreach (var segment in segments)
            {
                var idx = segment.IndexOf('=');
                if (idx <= 0) continue;
                var key = segment[..idx].Trim();
                if (key.Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                {
                    serverSegment = segment[(idx + 1)..].Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(serverSegment))
                return true;

            var host = serverSegment;
            var port = 1433;
            var commaIdx = serverSegment.IndexOf(',');
            if (commaIdx > 0)
            {
                host = serverSegment[..commaIdx].Trim();
                if (!int.TryParse(serverSegment[(commaIdx + 1)..].Trim(), out port))
                    port = 1433;
            }

            if (host.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
                host = host[4..].Trim();

            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(host, port);
            return connectTask.Wait(TimeSpan.FromMilliseconds(750)) && tcp.Connected;
        }
        catch
        {
            return false;
        }
    }
}
