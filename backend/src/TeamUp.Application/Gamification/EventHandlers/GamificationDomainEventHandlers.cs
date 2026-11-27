// Date: 2026-11-24
// Purpose: Domain event handlers wiring automatic gamification XP awards, badge evaluation, student inbox notifications, and EventNotification emails.

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Domain.Events;

namespace TeamUp.Application.Gamification.EventHandlers;

#region XP & Badge Handlers

public sealed class TaskCompletedDomainEventHandler(
    IAppDbContext db,
    IBadgeService badgeService,
    IOptions<GamificationOptions>? gamificationOptions = null)
    : IDomainEventHandler<TaskCompletedDomainEvent>
{
    private int PointsPerCompletedTaskValue => gamificationOptions?.Value.PointsPerCompletedTask ?? 50;

    [Obsolete("Use GamificationOptions.PointsPerCompletedTask; the value is configurable via the Gamification section.")]
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
            db, domainEvent.AssignedStudentId, PointsPerCompletedTaskValue,
            $"Completed task #{domainEvent.TaskId}: {domainEvent.TaskTitle}",
            ScoreSourceType.TaskCompleted, domainEvent.TaskId, cancellationToken);

        if (awarded)
            await badgeService.EvaluateStudentBadgesAsync(domainEvent.AssignedStudentId, cancellationToken);
    }
}

public sealed class SessionAttendedDomainEventHandler(
    IAppDbContext db,
    IBadgeService badgeService,
    IOptions<GamificationOptions>? gamificationOptions = null)
    : IDomainEventHandler<SessionAttendedDomainEvent>
{
    private int PointsPerSessionValue => gamificationOptions?.Value.PointsPerSession ?? 25;

    [Obsolete("Use GamificationOptions.PointsPerSession; the value is configurable via the Gamification section.")]
    public const int PointsPerSession = 25;

    public async Task HandleAsync(SessionAttendedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var awarded = await ScoreLedger.TryAwardAsync(
            db, domainEvent.StudentId, PointsPerSessionValue,
            $"Attended session #{domainEvent.SessionId}: {domainEvent.SessionTitle}",
            ScoreSourceType.EventAttendance, domainEvent.SessionId, cancellationToken);

        if (awarded)
            await badgeService.EvaluateStudentBadgesAsync(domainEvent.StudentId, cancellationToken);
    }
}

public sealed class ProjectCompletedDomainEventHandler(
    IAppDbContext db,
    IBadgeService badgeService,
    IOptions<GamificationOptions>? gamificationOptions = null)
    : IDomainEventHandler<ProjectCompletedDomainEvent>
{
    private int PointsPerProjectDeliveredValue => gamificationOptions?.Value.PointsPerProjectDelivered ?? 100;

    [Obsolete("Use GamificationOptions.PointsPerProjectDelivered; the value is configurable via the Gamification section.")]
    public const int PointsPerProjectDelivered = 100;

    public async Task HandleAsync(ProjectCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var reason = $"Delivered project #{domainEvent.ProjectId}: {domainEvent.ProjectTitle}";
        foreach (var studentId in domainEvent.MemberStudentIds.Distinct())
        {
            var awarded = await ScoreLedger.TryAwardAsync(
                db, studentId, PointsPerProjectDeliveredValue, reason,
                ScoreSourceType.ProjectDelivered, domainEvent.ProjectId, cancellationToken);

            if (awarded)
                await badgeService.EvaluateStudentBadgesAsync(studentId, cancellationToken);
        }
    }
}

#endregion

#region Notification & Email Handlers

public sealed class TaskAssignedDomainEventHandler(IAppDbContext db, TimeProvider? timeProvider = null)
    : IDomainEventHandler<TaskAssignedDomainEvent>
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public async Task HandleAsync(TaskAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification
        {
            StudentId = domainEvent.AssignedStudentId,
            Title = "New Task Assigned",
            Message = $"You have been assigned task '{domainEvent.TaskTitle}'.",
            Type = NotificationType.TaskAssigned,
            IsRead = false,
            CreatedAt = _time.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ApplicationReviewedDomainEventHandler(IAppDbContext db, TimeProvider? timeProvider = null)
    : IDomainEventHandler<ApplicationReviewedDomainEvent>
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

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
            CreatedAt = _time.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class WaitlistPromotedDomainEventHandler(
    IAppDbContext db,
    ITemplateRenderer? templateRenderer = null,
    IEmailSender? emailSender = null,
    IOptions<EmailOptions>? emailOptions = null,
    TimeProvider? timeProvider = null,
    ILogger<WaitlistPromotedDomainEventHandler>? logger = null)
    : IDomainEventHandler<WaitlistPromotedDomainEvent>
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public async Task HandleAsync(WaitlistPromotedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        db.Notifications.Add(new Notification
        {
            StudentId = domainEvent.StudentId,
            Title = "Event Spot Confirmed!",
            Message = $"A seat opened up and your registration for '{domainEvent.EventTitle}' is now confirmed.",
            Type = NotificationType.EventReminder,
            IsRead = false,
            CreatedAt = _time.GetUtcNow().UtcDateTime
        });
        await db.SaveChangesAsync(cancellationToken);

        await EventNotificationEmailHelper.TrySendEventNoticeAsync(
            db,
            templateRenderer,
            emailSender,
            emailOptions,
            domainEvent.EventId,
            domainEvent.StudentId,
            statusLabel: "PROMOTED FROM WAITLIST",
            summaryText: $"A seat opened up for '{domainEvent.EventTitle}' and your registration is now confirmed.",
            logger,
            cancellationToken);
    }
}

public sealed class EventRegisteredEmailDomainEventHandler(
    IAppDbContext db,
    ITemplateRenderer? templateRenderer = null,
    IEmailSender? emailSender = null,
    IOptions<EmailOptions>? emailOptions = null,
    ILogger<EventRegisteredEmailDomainEventHandler>? logger = null)
    : IDomainEventHandler<EventRegisteredDomainEvent>
{
    public async Task HandleAsync(EventRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var isConfirmed = domainEvent.Status == RegistrationStatus.Confirmed;
        var statusLabel = isConfirmed ? "SEAT CONFIRMED" : "WAITLISTED";
        var summaryText = isConfirmed
            ? $"Your seat for '{domainEvent.EventTitle}' is confirmed. Bring your TeamUp pass or scan the session QR code when check-in opens."
            : $"'{domainEvent.EventTitle}' is currently at capacity. You have been added to the automatic FIFO waitlist and will be promoted if a seat opens.";

        await EventNotificationEmailHelper.TrySendEventNoticeAsync(
            db,
            templateRenderer,
            emailSender,
            emailOptions,
            domainEvent.EventId,
            domainEvent.StudentId,
            statusLabel,
            summaryText,
            logger,
            cancellationToken);
    }
}

internal static partial class EventNotificationEmailHelper
{
    private static readonly TimeSpan EmailDispatchTimeout = TimeSpan.FromSeconds(8);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Best-effort event notification email failed for Event #{EventId}, Student #{StudentId} ({StatusLabel}).")]
    private static partial void LogEventNoticeFailed(
        ILogger logger, Exception ex, int eventId, int studentId, string statusLabel);

    public static async Task TrySendEventNoticeAsync(
        IAppDbContext db,
        ITemplateRenderer? templateRenderer,
        IEmailSender? emailSender,
        IOptions<EmailOptions>? emailOptions,
        int eventId,
        int studentId,
        string statusLabel,
        string summaryText,
        ILogger? logger,
        CancellationToken ct)
    {
        if (templateRenderer is null || emailSender is null)
            return;

        var student = await db.Students.AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new { s.FirstName, s.LastName, s.Email, s.EmailNotificationsEnabled })
            .FirstOrDefaultAsync(ct);

        if (student is null || !student.EmailNotificationsEnabled)
            return;

        var evt = await db.Events.AsNoTracking()
            .Where(e => e.EventId == eventId)
            .Select(e => new
            {
                e.EventId,
                e.Title,
                e.StartDate,
                e.EndDate,
                e.Location,
                OrganizationName = e.Organization.Name
            })
            .FirstOrDefaultAsync(ct);

        if (evt is null)
            return;

        var baseUrl = (emailOptions?.Value.FrontendBaseUrl ?? "https://teamup-ieee.vercel.app").TrimEnd('/');
        var values = new Dictionary<string, string>
        {
            ["RecipientName"] = $"{student.FirstName} {student.LastName}".Trim(),
            ["StatusLabel"] = statusLabel,
            ["EventMonth"] = evt.StartDate.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant(),
            ["EventDay"] = evt.StartDate.ToString("dd", CultureInfo.InvariantCulture),
            ["EventTitle"] = evt.Title,
            ["OrganizationName"] = evt.OrganizationName,
            ["EventDateRange"] = $"{evt.StartDate:MMM dd, yyyy HH:mm} - {evt.EndDate:MMM dd HH:mm} UTC",
            ["VenueLocation"] = evt.Location,
            ["SummaryText"] = summaryText,
            ["EventUrl"] = $"{baseUrl}/events/{evt.EventId}",
            ["PreferencesUrl"] = $"{baseUrl}/profile/me"
        };

        try
        {
            using var emailTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            emailTimeoutCts.CancelAfter(EmailDispatchTimeout);
            var emailCt = emailTimeoutCts.Token;

            var html = await templateRenderer.RenderEmailAsync("EventNotification.html", values, emailCt);
            await emailSender.SendTemplatedEmailAsync(
                new EmailDispatchRequest(
                    student.Email,
                    $"TeamUp Event Update · {evt.Title} ({statusLabel})",
                    html,
                    $"{statusLabel}: {evt.Title} ({evt.OrganizationName}) — {evt.Location}"),
                emailCt);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            if (logger is not null)
                LogEventNoticeFailed(logger, ex, eventId, studentId, statusLabel);
        }
    }
}

#endregion
