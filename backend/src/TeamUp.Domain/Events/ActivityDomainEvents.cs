// Date: 2026-11-26
// Purpose: Domain events dispatched automatically on SaveChanges (or post-commit for transactional operations) to drive gamification scoring, badge evaluation, and notifications.

using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Events;

public sealed record TaskCompletedDomainEvent(
    int TaskId,
    int ProjectId,
    int AssignedStudentId,
    string TaskTitle) : DomainEventBase;

public sealed record TaskAssignedDomainEvent(
    int TaskId,
    int ProjectId,
    int AssignedStudentId,
    string TaskTitle) : DomainEventBase;

public sealed record ProjectCompletedDomainEvent(
    int ProjectId,
    string ProjectTitle,
    IReadOnlyList<int> MemberStudentIds) : DomainEventBase;

public sealed record SessionAttendedDomainEvent(
    int SessionId,
    int EventId,
    int StudentId,
    string SessionTitle) : DomainEventBase;

public sealed record ApplicationReviewedDomainEvent(
    int StudentId,
    string TargetName,
    string TargetKind,
    ApplicationStatus Decision) : DomainEventBase;

public sealed record WaitlistPromotedDomainEvent(
    int EventId,
    int StudentId,
    string EventTitle) : DomainEventBase;

public sealed record EventRegisteredDomainEvent(
    int EventId,
    int StudentId,
    string EventTitle,
    RegistrationStatus Status) : DomainEventBase;
