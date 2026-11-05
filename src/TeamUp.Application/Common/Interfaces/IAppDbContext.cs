using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<University> Universities { get; }
    DbSet<Faculty> Faculties { get; }
    DbSet<Student> Students { get; }
    DbSet<StudentInterest> StudentInterests { get; }
    DbSet<StudentProfile> StudentProfiles { get; }

    DbSet<Skill> Skills { get; }
    DbSet<StudentSkill> StudentSkills { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectRequiredSkill> ProjectRequiredSkills { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<ProjectApplication> ProjectApplications { get; }
    DbSet<ProjectTask> ProjectTasks { get; }

    DbSet<Organization> Organizations { get; }
    DbSet<OrganizationMember> OrganizationMembers { get; }
    DbSet<MembershipApplication> MembershipApplications { get; }
    DbSet<Event> Events { get; }
    DbSet<EventRegistration> EventRegistrations { get; }
    DbSet<EventSession> EventSessions { get; }
    DbSet<SessionAttendance> SessionAttendances { get; }

    DbSet<ScoreTransaction> ScoreTransactions { get; }
    DbSet<Badge> Badges { get; }
    DbSet<StudentBadge> StudentBadges { get; }
    DbSet<Notification> Notifications { get; }

    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
