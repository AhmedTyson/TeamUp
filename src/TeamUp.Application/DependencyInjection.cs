using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Services;
using TeamUp.Application.Skills;
using TeamUp.Application.Students;
using TeamUp.Application.Universities;

namespace TeamUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<IStudentAccountService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentProfileService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentInterestService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<IProjectService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectSkillService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectMemberService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectTaskService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<ISkillService, SkillService>();

        // Module 03: Organizations & Events (Ibrahim - T20-T24)
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
