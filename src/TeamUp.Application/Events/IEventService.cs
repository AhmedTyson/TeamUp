using TeamUp.Application.Common;

namespace TeamUp.Application.Events;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetEventsAsync(string? search, int? organizationId, PageRequest page, CancellationToken ct = default);
    Task<EventDto> GetEventByIdAsync(int id, CancellationToken ct = default);
    Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken ct = default);
    Task<EventDto> UpdateEventAsync(int id, UpdateEventRequest request, CancellationToken ct = default);
    Task DeleteEventAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<EventSessionDto>> GetSessionsAsync(int eventId, CancellationToken ct = default);
    Task<EventSessionDto> CreateSessionAsync(int eventId, CreateEventSessionRequest request, CancellationToken ct = default);
    Task<EventSessionDto> UpdateSessionAsync(int sessionId, UpdateEventSessionRequest request, CancellationToken ct = default);
    Task DeleteSessionAsync(int sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<EventRegistrationDto>> GetRegistrationsAsync(int eventId, CancellationToken ct = default);
    Task<EventRegistrationDto> RegisterAsync(int eventId, int studentId, CancellationToken ct = default);
    Task CancelRegistrationAsync(int eventId, int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<SessionAttendanceDto>> GetAttendanceAsync(int sessionId, CancellationToken ct = default);
    Task<SessionAttendanceDto> CheckInAsync(int sessionId, CreateAttendanceRequest request, CancellationToken ct = default);
    Task RevokeAttendanceAsync(int sessionId, int studentId, CancellationToken ct = default);
}
