using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Projects;
using TeamUp.Application.Students;
using TeamUp.Application.Universities;
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
        services.AddSingleton<IPasswordHasher<Student>, PasswordHasher<Student>>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<IProfileService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentAccountService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentProfileService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentInterestService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddSingleton<SoftDeleteInterceptor>();

        var cs = config.GetConnectionString("TeamUpDb");
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
