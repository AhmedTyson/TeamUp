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
    TeamUp.Domain.Enums.RegistrationStatus Status);

public sealed record CreateAttendanceRequest(int StudentId, TeamUp.Domain.Enums.VerificationMethod VerificationMethod);

public sealed record SessionAttendanceDto(
    int SessionId,
    int StudentId,
    string FirstName,
    string LastName,
    DateTime CheckInTime,
    TeamUp.Domain.Enums.VerificationMethod VerificationMethod);
