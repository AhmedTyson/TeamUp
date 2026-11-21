// Date: 2026-11-21
// Purpose: Event, session, registration, attendance origin (RecordedByStudentId), and rotating QR/PIN check-in DTOs (STRIDE THR-T-02, THR-R-02).

using TeamUp.Domain.Enums;

namespace TeamUp.Application.Events;

public sealed record CreateEventRequest(
    string Title,
    string Description,
    EventType Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    int OrganizationId);

public sealed record UpdateEventRequest(
    string Title,
    string Description,
    EventType Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity);

public sealed record EventDto(
    int EventId,
    string Title,
    string Description,
    EventType Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    int OrganizationId,
    string OrganizationName,
    int RegisteredCount,
    IReadOnlyList<EventSessionDto> Sessions);

public sealed record CreateEventSessionRequest(
    int SessionNo,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string? Room);

public sealed record UpdateEventSessionRequest(
    int SessionNo,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string? Room);

public sealed record EventSessionDto(
    int SessionId,
    int EventId,
    int SessionNo,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string? Room);

public sealed record EventRegistrationDto(
    int EventId,
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    DateTime RegisteredAt,
    RegistrationStatus Status);

public sealed record CreateAttendanceRequest(
    int StudentId,
    VerificationMethod VerificationMethod,
    string? VerificationCode = null);

public sealed record SelfCheckInRequest(
    string VerificationCode);

public sealed record SessionCheckInCodeDto(
    int SessionId,
    string Code,
    DateTime ExpiresAtUtc);

public sealed record SessionAttendanceDto(
    int SessionId,
    int StudentId,
    string FirstName,
    string LastName,
    DateTime CheckInTime,
    VerificationMethod VerificationMethod,
    int? RecordedByStudentId = null);
