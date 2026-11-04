using System.ComponentModel.DataAnnotations;

namespace TeamUp.Api.Models.DTOs;

public record EventSessionCreateDto(
    [Required, StringLength(200, MinimumLength = 2)] string Title,
    string? Speaker,
    [Required, StringLength(120, MinimumLength = 1)] string Room,
    DateTime StartTime,
    DateTime EndTime
);

public record EventSessionResponseDto(
    string SessionId,
    string EventId,
    string Title,
    string? Speaker,
    string Room,
    DateTime StartTime,
    DateTime EndTime,
    bool IsDeleted
);

public record EventCreateDto(
    [Required] string OrganizationId,
    [Required, StringLength(200, MinimumLength = 2)] string Title,
    [Required, MinLength(5)] string Description,
    [Required, RegularExpression("^(Workshop|Hackathon|Competition|Seminar|Bootcamp)$")] string Type,
    [Required, StringLength(255, MinimumLength = 2)] string Location,
    DateTime StartDate,
    DateTime EndDate,
    [Range(1, int.MaxValue)] int Capacity,
    List<EventSessionCreateDto>? Sessions = null
);

public record EventUpdateDto(
    [StringLength(200, MinimumLength = 2)] string? Title,
    [MinLength(5)] string? Description,
    [StringLength(255, MinimumLength = 2)] string? Location,
    DateTime? StartDate,
    DateTime? EndDate,
    [Range(1, int.MaxValue)] int? Capacity
);

public record EventResponseDto(
    string EventId,
    string OrganizationId,
    string OrganizationName,
    string Title,
    string Description,
    string Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    int RegisteredCount,
    bool IsDeleted,
    DateTime CreatedAt,
    List<EventSessionResponseDto>? Sessions = null
);

public record EventListResponseDto(
    List<EventResponseDto> Items,
    int TotalCount,
    int Page,
    int Limit
);

