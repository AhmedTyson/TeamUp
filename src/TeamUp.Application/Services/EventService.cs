using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Events;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Application.Services;

public sealed class EventService(IAppDbContext db) : IEventService
{
    public async Task<PagedResult<EventDto>> GetEventsAsync(string? search, int? organizationId, PageRequest page, CancellationToken ct = default)
    {
        var query = db.Events.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Description.Contains(term) || x.Location.Contains(term));
        }
        if (organizationId.HasValue)
            query = query.Where(x => x.OrganizationId == organizationId.Value);

        var total = await query.CountAsync(ct);
        var events = await query.OrderBy(x => x.StartDate).ThenBy(x => x.Title)
            .Skip(page.Skip).Take(page.SafePageSize)
            .Include(x => x.Organization)
            .Include(x => x.Sessions)
            .ToListAsync(ct);
        var ids = events.Select(x => x.EventId).ToArray();
        var counts = await db.EventRegistrations.AsNoTracking()
            .Where(x => ids.Contains(x.EventId) && x.Status == RegistrationStatus.Confirmed)
            .GroupBy(x => x.EventId)
            .Select(x => new { EventId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count, ct);
        var items = events.Select(x => ToEventDto(x, counts.GetValueOrDefault(x.EventId))).ToList();
        return new PagedResult<EventDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<EventDto> GetEventByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Events.AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.EventId == id, ct)
            ?? throw new NotFoundException($"Event with ID {id} was not found.");
        var count = await db.EventRegistrations.CountAsync(
            x => x.EventId == id && x.Status == RegistrationStatus.Confirmed, ct);
        return ToEventDto(entity, count);
    }

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken ct = default)
    {
        if (request.EndDate < request.StartDate)
            throw new BadRequestException("Event EndDate must be greater than or equal to StartDate.");
        if (request.Capacity < 1)
            throw new BadRequestException("Event capacity must be at least 1.");
        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization with ID {request.OrganizationId} was not found.");

        var entity = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Location = request.Location.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            OrganizationId = organization.OrganizationId
        };
        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);
        entity.Organization = organization;
        return ToEventDto(entity, 0);
    }

    public async Task<EventDto> UpdateEventAsync(int id, UpdateEventRequest request, CancellationToken ct = default)
    {
        if (request.EndDate < request.StartDate)
            throw new BadRequestException("Event EndDate must be greater than or equal to StartDate.");
        if (request.Capacity < 1)
            throw new BadRequestException("Event capacity must be at least 1.");
        var entity = await db.Events.FirstOrDefaultAsync(x => x.EventId == id, ct)
            ?? throw new NotFoundException($"Event with ID {id} was not found.");
        var confirmed = await db.EventRegistrations.CountAsync(
            x => x.EventId == id && x.Status == RegistrationStatus.Confirmed, ct);
        if (request.Capacity < confirmed)
            throw new ConflictException("Capacity cannot be lower than the current number of confirmed registrations.");

        entity.Title = request.Title.Trim();
        entity.Description = request.Description.Trim();
        entity.Type = request.Type;
        entity.Location = request.Location.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Capacity = request.Capacity;
        await db.SaveChangesAsync(ct);
        return await GetEventByIdAsync(id, ct);
    }

    public async Task DeleteEventAsync(int id, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var entity = await db.Events.Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.EventId == id, ct)
            ?? throw new NotFoundException($"Event with ID {id} was not found.");
        foreach (var session in entity.Sessions)
            db.EventSessions.Remove(session);
        db.Events.Remove(entity);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<EventSessionDto>> GetSessionsAsync(int eventId, CancellationToken ct = default)
    {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId, ct))
            throw new NotFoundException($"Event with ID {eventId} was not found.");
        return await db.EventSessions.AsNoTracking().Where(x => x.EventId == eventId)
            .OrderBy(x => x.SessionNo)
            .Select(x => new EventSessionDto(x.SessionId, x.EventId, x.SessionNo, x.Title, x.StartTime, x.EndTime, x.Room))
            .ToListAsync(ct);
    }

    public async Task<EventSessionDto> CreateSessionAsync(int eventId, CreateEventSessionRequest request, CancellationToken ct = default)
    {
        var parent = await db.Events.FirstOrDefaultAsync(x => x.EventId == eventId, ct)
            ?? throw new NotFoundException($"Event with ID {eventId} was not found.");
        ValidateSessionWindow(parent, request.StartTime, request.EndTime);
        if (await db.EventSessions.AnyAsync(x => x.EventId == eventId && x.SessionNo == request.SessionNo, ct))
            throw new ConflictException($"Session number {request.SessionNo} already exists for this event.");
        var session = new EventSession
        {
            EventId = eventId,
            SessionNo = request.SessionNo,
            Title = request.Title.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Room = NormalizeOptional(request.Room)
        };
        db.EventSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return ToSessionDto(session);
    }

    public async Task<EventSessionDto> UpdateSessionAsync(int sessionId, UpdateEventSessionRequest request, CancellationToken ct = default)
    {
        var session = await db.EventSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct)
            ?? throw new NotFoundException($"Session with ID {sessionId} was not found.");
        var parent = await db.Events.FirstOrDefaultAsync(x => x.EventId == session.EventId, ct)
            ?? throw new NotFoundException($"Event with ID {session.EventId} was not found.");
        ValidateSessionWindow(parent, request.StartTime, request.EndTime);
        if (await db.EventSessions.AnyAsync(x => x.EventId == session.EventId && x.SessionId != sessionId
            && x.SessionNo == request.SessionNo, ct))
            throw new ConflictException($"Session number {request.SessionNo} already exists for this event.");
        session.SessionNo = request.SessionNo;
        session.Title = request.Title.Trim();
        session.StartTime = request.StartTime;
        session.EndTime = request.EndTime;
        session.Room = NormalizeOptional(request.Room);
        await db.SaveChangesAsync(ct);
        return ToSessionDto(session);
    }

    public async Task DeleteSessionAsync(int sessionId, CancellationToken ct = default)
    {
        var session = await db.EventSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct)
            ?? throw new NotFoundException($"Session with ID {sessionId} was not found.");
        db.EventSessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }


    public async Task<IReadOnlyList<EventRegistrationDto>> GetRegistrationsAsync(int eventId, CancellationToken ct = default)
    {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId, ct))
            throw new NotFoundException($"Event with ID {eventId} was not found.");
        return await db.EventRegistrations.AsNoTracking()
            .Where(x => x.EventId == eventId && x.Status != RegistrationStatus.Cancelled)
            .OrderBy(x => x.Status).ThenBy(x => x.RegisteredAt)
            .Select(x => new EventRegistrationDto(x.EventId, x.StudentId,
                x.Student.FirstName, x.Student.LastName, x.Student.Email, x.RegisteredAt, x.Status))
            .ToListAsync(ct);
    }

    public async Task<EventRegistrationDto> RegisterAsync(int eventId, int studentId, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var eventEntity = await db.Events.FirstOrDefaultAsync(x => x.EventId == eventId, ct)
            ?? throw new NotFoundException($"Event with ID {eventId} was not found.");
        var student = await db.Students.FirstOrDefaultAsync(x => x.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");
        var existing = await db.EventRegistrations.IgnoreQueryFilters().FirstOrDefaultAsync(
            x => x.EventId == eventId && x.StudentId == studentId, ct);
        if (existing is not null && existing.DeletedAt is null && existing.Status != RegistrationStatus.Cancelled)
            throw new ConflictException("You already have an active registration for this event.");

        var confirmedCount = await db.EventRegistrations.CountAsync(
            x => x.EventId == eventId && x.Status == RegistrationStatus.Confirmed, ct);
        var status = confirmedCount < eventEntity.Capacity
            ? RegistrationStatus.Confirmed
            : RegistrationStatus.Waitlisted;

        if (existing is null)
        {
            existing = new EventRegistration
            {
                EventId = eventId,
                StudentId = studentId,
                RegisteredAt = DateTime.UtcNow,
                Status = status
            };
            db.EventRegistrations.Add(existing);
        }
        else
        {
            existing.DeletedAt = null;
            existing.RegisteredAt = DateTime.UtcNow;
            existing.Status = status;
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new EventRegistrationDto(eventId, studentId, student.FirstName, student.LastName,
            student.Email, existing.RegisteredAt, existing.Status);
    }

    public async Task CancelRegistrationAsync(int eventId, int studentId, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var registration = await db.EventRegistrations.FirstOrDefaultAsync(
            x => x.EventId == eventId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("Event registration was not found.");
        if (registration.Status == RegistrationStatus.Cancelled)
            throw new ConflictException("This event registration has already been cancelled.");

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;
        registration.Status = RegistrationStatus.Cancelled;
        registration.DeletedAt = DateTime.UtcNow;
        if (wasConfirmed)
        {
            var next = await db.EventRegistrations
                .Where(x => x.EventId == eventId && x.Status == RegistrationStatus.Waitlisted)
                .OrderBy(x => x.RegisteredAt)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
                next.Status = RegistrationStatus.Confirmed;
        }
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<SessionAttendanceDto>> GetAttendanceAsync(int sessionId, CancellationToken ct = default)
    {
        if (!await db.EventSessions.AnyAsync(x => x.SessionId == sessionId, ct))
            throw new NotFoundException($"Session with ID {sessionId} was not found.");
        return await db.SessionAttendances.AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CheckInTime)
            .Select(x => new SessionAttendanceDto(x.SessionId, x.StudentId,
                x.Student.FirstName, x.Student.LastName, x.CheckInTime, x.VerificationMethod))
            .ToListAsync(ct);
    }

    public async Task<SessionAttendanceDto> CheckInAsync(int sessionId, CreateAttendanceRequest request, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.VerificationMethod))
            throw new BadRequestException("Invalid attendance verification method.");
        var sessionExists = await db.EventSessions.AnyAsync(x => x.SessionId == sessionId, ct);
        if (!sessionExists)
            throw new NotFoundException($"Session with ID {sessionId} was not found.");
        var student = await db.Students.FirstOrDefaultAsync(x => x.StudentId == request.StudentId, ct)
            ?? throw new NotFoundException($"Student with ID {request.StudentId} was not found.");
        var session = await db.EventSessions.FirstAsync(x => x.SessionId == sessionId, ct);
        var isRegistered = await db.EventRegistrations.AnyAsync(x => x.EventId == session.EventId
            && x.StudentId == request.StudentId && x.Status == RegistrationStatus.Confirmed, ct);
        if (!isRegistered)
            throw new BadRequestException("Only students with a confirmed event registration can check in.");
        if (await db.SessionAttendances.IgnoreQueryFilters().AnyAsync(x => x.SessionId == sessionId && x.StudentId == request.StudentId, ct))
            throw new ConflictException("The student has already checked in to this session.");

        var attendance = new SessionAttendance
        {
            SessionId = sessionId,
            StudentId = request.StudentId,
            CheckInTime = DateTime.UtcNow,
            VerificationMethod = request.VerificationMethod
        };
        db.SessionAttendances.Add(attendance);
        await db.SaveChangesAsync(ct);
        return new SessionAttendanceDto(sessionId, request.StudentId, student.FirstName,
            student.LastName, attendance.CheckInTime, attendance.VerificationMethod);
    }

    public async Task RevokeAttendanceAsync(int sessionId, int studentId, CancellationToken ct = default)
    {
        var attendance = await db.SessionAttendances.FirstOrDefaultAsync(
            x => x.SessionId == sessionId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("Session attendance record was not found.");
        db.SessionAttendances.Remove(attendance);
        await db.SaveChangesAsync(ct);
    }

    private static void ValidateSessionWindow(Event parent, DateTime start, DateTime end)
    {
        if (end < start)
            throw new BadRequestException("Session EndTime must be greater than or equal to StartTime.");
        if (start < parent.StartDate || end > parent.EndDate)
            throw new BadRequestException("Session time must be within the parent event date range.");
    }

    private static EventDto ToEventDto(Event entity, int registeredCount) => new(
        entity.EventId, entity.Title, entity.Description, entity.Type, entity.Location,
        entity.StartDate, entity.EndDate, entity.Capacity, entity.OrganizationId,
        entity.Organization.Name, registeredCount,
        entity.Sessions.OrderBy(x => x.SessionNo).Select(ToSessionDto).ToList());

    private static EventSessionDto ToSessionDto(EventSession session) => new(
        session.SessionId, session.EventId, session.SessionNo, session.Title,
        session.StartTime, session.EndTime, session.Room);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
