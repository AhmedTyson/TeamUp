namespace TeamUp.Api.Models.DTOs;

public record RegistrationStudentSummaryDto(
    string StudentId,
    string Email,
    string FirstName,
    string LastName
);

public record EventRegistrationResponseDto(
    string RegistrationId,
    string EventId,
    string StudentId,
    string Status, // Confirmed, Waitlisted, Cancelled
    DateTime RegisteredAt,
    DateTime? CancelledAt = null,
    RegistrationStudentSummaryDto? Student = null
);

public record EventRegistrationListResponseDto(
    List<EventRegistrationResponseDto> Items,
    int TotalCount,
    int Page,
    int Limit
);

