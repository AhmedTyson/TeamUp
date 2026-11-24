// Date: 2026-11-21
// Purpose: Event, session, registration, attendance, rotating check-in code, and printable report service contract (STRIDE THR-T-01, THR-T-02, THR-R-02).

using TeamUp.Application.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Events;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetEventsAsync(
        string? search,
        int? organizationId,
        PageRequest page,
        EventType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        EventDeliveryMode? deliveryMode = null,
        CancellationToken ct = default);

    Task<PagedResult<EventDto>> GetPublicEventsAsync(
        string? search,
        int? organizationId,
        PageRequest page,
        EventType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        EventDeliveryMode? deliveryMode = null,
        CancellationToken ct = default);

    Task<EventDto> GetEventByIdAsync(int id, int actorStudentId = 0, string? actorRole = null, CancellationToken ct = default);
    Task<EventDto> GetPublicEventByIdAsync(int id, CancellationToken ct = default);
    Task<EventDto> CreateEventAsync(int actorStudentId, string? actorRole, CreateEventRequest request, CancellationToken ct = default);
    Task<EventDto> UpdateEventAsync(int id, int actorStudentId, string? actorRole, UpdateEventRequest request, CancellationToken ct = default);
    Task DeleteEventAsync(int id, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<IReadOnlyList<EventSessionDto>> GetSessionsAsync(int eventId, CancellationToken ct = default);
    Task<EventSessionDto> CreateSessionAsync(int eventId, int actorStudentId, string? actorRole, CreateEventSessionRequest request, CancellationToken ct = default);
    Task<EventSessionDto> UpdateSessionAsync(int sessionId, int actorStudentId, string? actorRole, UpdateEventSessionRequest request, CancellationToken ct = default);
    Task DeleteSessionAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<IReadOnlyList<MyEventRegistrationDto>> GetMyRegistrationsAsync(int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<EventRegistrationDto>> GetRegistrationsAsync(int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<EventRegistrationDto> RegisterAsync(int eventId, int studentId, CancellationToken ct = default);
    Task CancelRegistrationAsync(int eventId, int targetStudentId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<IReadOnlyList<SessionAttendanceDto>> GetAttendanceAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<SessionCheckInCodeDto> GenerateCheckInCodeAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<SessionAttendanceDto> CheckInAsync(int sessionId, int actorStudentId, string? actorRole, CreateAttendanceRequest request, CancellationToken ct = default);
    Task<SessionAttendanceDto> SelfCheckInAsync(int sessionId, int studentId, string verificationCode, CancellationToken ct = default);
    Task RevokeAttendanceAsync(int sessionId, int studentId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<string> RenderAttendanceReportHtmlAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<string> ExportAttendanceCsvAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
}
