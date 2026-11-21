// Date: 2026-11-21
// Purpose: Application layer DI registrations for services, validators, security guards, caching, and domain event handlers.

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Gamification.EventHandlers;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Services;
using TeamUp.Application.Skills;
using TeamUp.Application.Students;
using TeamUp.Application.Universities;
using TeamUp.Domain.Events;

namespace TeamUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMemoryCache();

        // Security, multi-tenant authorization, rotating check-in tokens, and audit logging
        services.TryAddScoped<IRequestContext, NullRequestContext>();
        services.AddScoped<IOrganizationAuthorizationService, OrganizationAuthorizationService>();
        services.AddScoped<ISecurityStampValidator, SecurityStampValidator>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IAttendanceCodeService, AttendanceCodeService>();

        // Module 01: Auth, Identity, Profiles, Academic Catalog & Admin
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<IStudentAccountService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentProfileService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IStudentInterestService>(sp => sp.GetRequiredService<ProfileService>());
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<IAdminService, AdminService>();

        // Module 02: Skills, Projects, Teams & Sprint Tasks
        services.AddScoped<ProjectService>();
        services.AddScoped<IProjectService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectSkillService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectMemberService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<IProjectTaskService>(sp => sp.GetRequiredService<ProjectService>());
        services.AddScoped<ISkillService, SkillService>();

        // Module 03: Organizations, Events, Registrations & Attendance
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IEventService, EventService>();

        // Module 04: Gamification, Badges & Notifications
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Domain event handlers (automatic XP awards, badge evaluation & notifications)
        services.AddScoped<IDomainEventHandler<TaskCompletedDomainEvent>, TaskCompletedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<SessionAttendedDomainEvent>, SessionAttendedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ProjectCompletedDomainEvent>, ProjectCompletedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<TaskAssignedDomainEvent>, TaskAssignedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ApplicationReviewedDomainEvent>, ApplicationReviewedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<WaitlistPromotedDomainEvent>, WaitlistPromotedDomainEventHandler>();

        return services;
    }
}
