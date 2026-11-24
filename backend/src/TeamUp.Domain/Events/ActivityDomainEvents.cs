// Date: 2026-11-21
// Purpose: Domain events dispatched automatically on SaveChanges to drive gamification scoring, badge evaluation, and notifications.

using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Events;

public sealed record TaskCompletedDomainEvent(
    int TaskId,
    int ProjectId,
    int AssignedStudentId,
    string TaskTitle) : IDomainEvent;

public sealed record TaskAssignedDomainEvent(
    int TaskId,
    int ProjectId,
    int AssignedStudentId,
    string TaskTitle) : IDomainEvent;

public sealed record ProjectCompletedDomainEvent(
    int ProjectId,
    string ProjectTitle,
    IReadOnlyList<int> MemberStudentIds) : IDomainEvent;

public sealed record SessionAttendedDomainEvent(
    int SessionId,
    int EventId,
    int StudentId,
    string SessionTitle) : IDomainEvent;

public sealed record ApplicationReviewedDomainEvent(
    int StudentId,
    string TargetName,
    string TargetKind,
    ApplicationStatus Decision) : IDomainEvent;

public sealed record WaitlistPromotedDomainEvent(
    int EventId,
    int StudentId,
    string EventTitle) : IDomainEvent;

public sealed record EventRegisteredDomainEvent(
    int EventId,
    int StudentId,
    string EventTitle,
    RegistrationStatus Status) : IDomainEvent;
