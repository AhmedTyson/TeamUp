// Date: 2026-11-21
// Purpose: Domain event handlers wiring automatic gamification XP awards, badge evaluation, and student inbox notifications.

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Domain.Events;

namespace TeamUp.Application.Gamification.EventHandlers;

public sealed class TaskCompletedDomainEventHandler(IAppDbContext db, IBadgeService badgeService)
    : IDomainEventHandler<TaskCompletedDomainEvent>
{
    public const int PointsPerCompletedTask = 50;

    public async Task HandleAsync(TaskCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // A task pays out once in its lifetime, whoever it is (re)assigned to afterwards.
        var alreadyPaid = await db.ScoreTransactions.AsNoTracking().AnyAsync(
            t => t.SourceType == ScoreSourceType.TaskCompleted && t.SourceReferenceId == domainEvent.TaskId,
            cancellationToken);
        if (alreadyPaid)
            return;

        var awarded = await ScoreLedger.TryAwardAsync(
            db, domainEvent.AssignedStudentId, PointsPerCompletedTask,
            $"Completed task #{domainEvent.TaskId}: {domainEvent.TaskTitle}",
            ScoreSourceType.TaskCompleted, domainEvent.TaskId, cancellationToken);

        if (awarded)
            await badgeService.EvaluateStudentBadgesAsync(domainEvent.AssignedStudentId, cancellationToken);
    }
}

public sealed class SessionAttendedDomainEventHandler(IAppDbContext db, IBadgeService badgeService)
    : IDomainEventHandler<SessionAttendedDomainEvent>
{
    public const int PointsPerSession = 25;

    public async Task HandleAsync(SessionAttendedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var awarded = await ScoreLedger.TryAwardAsync(
            db, domainEvent.StudentId, PointsPerSession,
            $"Attended session #{domainEvent.SessionId}: {domainEvent.SessionTitle}",
            ScoreSourceType.EventAttendance, domainEvent.SessionId, cancellationToken);

        if (awarded)
            await badgeService.EvaluateStudentBadgesAsync(domainEvent.StudentId, cancellationToken);
    }
}

public sealed class ProjectCompletedDomainEventHandler(IAppDbContext db, IBadgeService badgeService)
    : IDomainEventHandler<ProjectCompletedDomainEvent>
{
    public const int PointsPerProjectDelivered = 100;

    public async Task HandleAsync(ProjectCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var reason = $"Delivered project #{domainEvent.ProjectId}: {domainEvent.ProjectTitle}";
        foreach (var studentId in domainEvent.MemberStudentIds.Distinct())
        {
            var awarded = await ScoreLedger.TryAwardAsync(
                db, studentId, PointsPerProjectDelivered, reason,
                ScoreSourceType.ProjectDelivered, domainEvent.ProjectId, cancellationToken);

            if (awarded)
                await badgeService.EvaluateStudentBadgesAsync(studentId, cancellationToken);
        }
    }
}

public sealed class TaskAssignedDomainEventHandler(IAppDbContext db)
    : IDomainEventHandler<TaskAssignedDomainEvent>
{
    public async Task HandleAsync(TaskAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification
        {
            StudentId = domainEvent.AssignedStudentId,
            Title = "New Task Assigned",
            Message = $"You have been assigned task '{domainEvent.TaskTitle}'.",
            Type = NotificationType.TaskAssigned,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ApplicationReviewedDomainEventHandler(IAppDbContext db)
    : IDomainEventHandler<ApplicationReviewedDomainEvent>
{
    public async Task HandleAsync(ApplicationReviewedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var decisionWord = domainEvent.Decision == ApplicationStatus.Accepted ? "accepted" : "rejected";
        db.Notifications.Add(new Notification
        {
            StudentId = domainEvent.StudentId,
            Title = $"{domainEvent.TargetKind} Application Update",
            Message = $"Your application to join '{domainEvent.TargetName}' was {decisionWord}.",
            Type = NotificationType.ApplicationStatus,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class WaitlistPromotedDomainEventHandler(IAppDbContext db)
    : IDomainEventHandler<WaitlistPromotedDomainEvent>
{
    public async Task HandleAsync(WaitlistPromotedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification
        {
            StudentId = domainEvent.StudentId,
            Title = "Event Spot Confirmed!",
            Message = $"A seat opened up and your registration for '{domainEvent.EventTitle}' is now confirmed.",
            Type = NotificationType.EventReminder,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
