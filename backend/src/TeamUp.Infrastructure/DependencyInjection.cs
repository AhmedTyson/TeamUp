// Date: 2026-11-21
// Purpose: Infrastructure DI registration wiring EF Core, JWT signing, SMTP/dev email transport, templates, and domain event dispatching.

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

        if (!provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(cs))
        {
            services.AddDbContext<AppDbContext>((sp, o) => o
                .UseSqlServer(cs, sql => sql.EnableRetryOnFailure())
                .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
        }

        return services;
    }
}
