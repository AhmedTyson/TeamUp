// Date: 2026-11-24
// Purpose: Event, session, waitlist-locking registration, student cross-device registration summary, public preview, CSV attendance export, and email dispatch events (STRIDE THR-T-01, THR-T-02, THR-R-02, THR-D-02).

using System.Data;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Images;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Events;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Domain.Events;

namespace TeamUp.Application.Services;

public sealed class EventService(
    IAppDbContext db,
    IOrganizationAuthorizationService orgAuth,
    IAttendanceCodeService attendanceCodes,
    ITemplateRenderer templateRenderer,
    IOptions<AttendanceOptions> attendanceOptions,
    TimeProvider? timeProvider = null) : IEventService
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    #region Event Discovery & CRUD

    public Task<PagedResult<EventDto>> GetEventsAsync(
        string? search,
        int? organizationId,
        PageRequest page,
        EventType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        EventDeliveryMode? deliveryMode = null,
        CancellationToken ct = default) =>
        QueryEventsCoreAsync(search, organizationId, page, type, from, to, deliveryMode, publicOnly: false, ct);

    public Task<PagedResult<EventDto>> GetPublicEventsAsync(
        string? search,
        int? organizationId,
        PageRequest page,
        EventType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        EventDeliveryMode? deliveryMode = null,
        CancellationToken ct = default) =>
        QueryEventsCoreAsync(search, organizationId, page, type, from, to, deliveryMode, publicOnly: true, ct);

    private async Task<PagedResult<EventDto>> QueryEventsCoreAsync(
        string? search,
        int? organizationId,
        PageRequest page,
        EventType? type,
        DateTime? from,
        DateTime? to,
        EventDeliveryMode? deliveryMode,
        bool publicOnly,
        CancellationToken ct)
    {
        var query = db.Events.AsNoTracking();
        if (publicOnly)
            query = query.Where(x => x.IsPublic);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Description.Contains(term) || x.Location.Contains(term));
        }
        if (organizationId.HasValue)
            query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);
        if (deliveryMode.HasValue)
            query = query.Where(x => x.DeliveryMode == deliveryMode.Value);
        if (from.HasValue)
            query = query.Where(x => x.EndDate >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.StartDate <= to.Value);

        var total = await query.CountAsync(ct);
        var events = await query.OrderBy(x => x.StartDate).ThenBy(x => x.Title)
            .Skip(page.Skip).Take(page.SafePageSize)
            .Include(x => x.Organization)
            .Include(x => x.Unit)
            .Include(x => x.Sessions)
            .AsSplitQuery()
            .ToListAsync(ct);

        var ids = events.Select(x => x.EventId).ToArray();
        var counts = await db.EventRegistrations.AsNoTracking()
            .Where(x => ids.Contains(x.EventId) && x.Status == RegistrationStatus.Confirmed)
            .GroupBy(x => x.EventId)
            .Select(x => new { EventId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count, ct);

        // Mask MeetingUrl in list responses to prevent unauthorized link scraping
        var items = events.Select(x => ToEventDto(x, counts.GetValueOrDefault(x.EventId), exposeMeetingUrl: false)).ToList();
        return new PagedResult<EventDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<EventDto> GetEventByIdAsync(
        int id, int actorStudentId = 0, string? actorRole = null, CancellationToken ct = default)
    {
        var entity = await db.Events.AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Unit)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.EventId == id, ct)
            ?? throw new NotFoundException($"Event with ID {id} was not found.");

        var count = await db.EventRegistrations.CountAsync(
            x => x.EventId == id && x.Status == RegistrationStatus.Confirmed, ct);

        var exposeMeetingUrl = await CanAccessMeetingUrlAsync(entity, actorStudentId, actorRole, ct);
        return ToEventDto(entity, count, exposeMeetingUrl);
    }

    public async Task<EventDto> GetPublicEventByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Events.AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Unit)
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x => x.EventId == id && x.IsPublic, ct)
            ?? throw new NotFoundException($"Event with ID {id} was not found.");

        var count = await db.EventRegistrations.CountAsync(
            x => x.EventId == id && x.Status == RegistrationStatus.Confirmed, ct);

        return ToEventDto(entity, count, exposeMeetingUrl: false);
    }

    private async Task<bool> CanAccessMeetingUrlAsync(
        Event entity, int actorStudentId, string? actorRole, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entity.MeetingUrl))
            return false;
        if (string.Equals(actorRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase))
            return true;
        if (actorStudentId <= 0)
            return false;
        if (await orgAuth.IsOrgAdminAsync(entity.OrganizationId, actorStudentId, ct))
            return true;
        return await db.EventRegistrations.AsNoTracking().AnyAsync(
            r => r.EventId == entity.EventId && r.StudentId == actorStudentId && r.Status == RegistrationStatus.Confirmed, ct);
    }

    public async Task<EventDto> CreateEventAsync(
        int actorStudentId, string? actorRole, CreateEventRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageOrganizationAsync(request.OrganizationId, actorStudentId, actorRole, ct);

        if (request.EndDate < request.StartDate)
            throw new BadRequestException("Event EndDate must be greater than or equal to StartDate.");
        if (request.Capacity < 1)
            throw new BadRequestException("Event capacity must be at least 1.");

        var organization = await db.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == request.OrganizationId, ct)
            ?? throw new NotFoundException($"Organization with ID {request.OrganizationId} was not found.");

        OrganizationUnit? unit = null;
        if (request.OrganizationUnitId is > 0)
        {
            unit = await db.OrganizationUnits.FirstOrDefaultAsync(
                u => u.OrganizationId == organization.OrganizationId && u.OrganizationUnitId == request.OrganizationUnitId.Value, ct)
                ?? throw new NotFoundException($"Organization unit #{request.OrganizationUnitId.Value} was not found.");
        }

        var entity = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Type = request.Type,
            Location = request.Location.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            OrganizationId = organization.OrganizationId,
            DeliveryMode = request.DeliveryMode,
            IsPublic = request.IsPublic,
            MeetingUrl = NormalizeOptional(request.MeetingUrl),
            OrganizationUnitId = unit?.OrganizationUnitId
        };
        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);
        entity.Organization = organization;
        entity.Unit = unit;
        return ToEventDto(entity, 0, exposeMeetingUrl: true);
    }

    public async Task<EventDto> UpdateEventAsync(
        int id, int actorStudentId, string? actorRole, UpdateEventRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageEventAsync(id, actorStudentId, actorRole, ct);

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

        if (request.OrganizationUnitId is > 0 && !await db.OrganizationUnits.AnyAsync(
            u => u.OrganizationId == entity.OrganizationId && u.OrganizationUnitId == request.OrganizationUnitId.Value, ct))
        {
            throw new NotFoundException($"Organization unit #{request.OrganizationUnitId.Value} was not found.");
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description.Trim();
        entity.Type = request.Type;
        entity.Location = request.Location.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Capacity = request.Capacity;
        entity.DeliveryMode = request.DeliveryMode;
        entity.IsPublic = request.IsPublic;
        entity.MeetingUrl = NormalizeOptional(request.MeetingUrl);
        entity.OrganizationUnitId = request.OrganizationUnitId is > 0 ? request.OrganizationUnitId : null;
        await db.SaveChangesAsync(ct);

        return await GetEventByIdAsync(id, actorStudentId, actorRole, ct);
    }

    public async Task DeleteEventAsync(int id, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageEventAsync(id, actorStudentId, actorRole, ct);

        await db.ExecuteInTransactionAsync(IsolationLevel.ReadCommitted, async token =>
        {
            var entity = await db.Events.Include(x => x.Sessions)
                .FirstOrDefaultAsync(x => x.EventId == id, token)
                ?? throw new NotFoundException($"Event with ID {id} was not found.");
            foreach (var session in entity.Sessions)
                db.EventSessions.Remove(session);
            db.Events.Remove(entity);
            await db.SaveChangesAsync(token);
        }, ct);
    }

    #endregion

    #region Event Sessions

    public async Task<IReadOnlyList<EventSessionDto>> GetSessionsAsync(int eventId, CancellationToken ct = default)
    {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId, ct))
            throw new NotFoundException($"Event with ID {eventId} was not found.");
        return await db.EventSessions.AsNoTracking().Where(x => x.EventId == eventId)
            .OrderBy(x => x.SessionNo)
            .Select(x => new EventSessionDto(x.SessionId, x.EventId, x.SessionNo, x.Title, x.StartTime, x.EndTime, x.Room))
            .ToListAsync(ct);
    }

    public async Task<EventSessionDto> CreateSessionAsync(
        int eventId, int actorStudentId, string? actorRole, CreateEventSessionRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageEventAsync(eventId, actorStudentId, actorRole, ct);

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

    public async Task<EventSessionDto> UpdateSessionAsync(
        int sessionId, int actorStudentId, string? actorRole, UpdateEventSessionRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

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

    public async Task DeleteSessionAsync(int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

        var session = await db.EventSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct)
            ?? throw new NotFoundException($"Session with ID {sessionId} was not found.");
        db.EventSessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }

    #endregion

    #region Registrations & Waitlist

    public async Task<PagedResult<MyEventRegistrationDto>> GetMyRegistrationsAsync(
        int studentId, PageRequest page, CancellationToken ct = default)
    {
        var query = db.EventRegistrations.AsNoTracking()
            .Where(r => r.StudentId == studentId && r.Status != RegistrationStatus.Cancelled);

        var total = await query.CountAsync(ct);
        var regs = await query
            .OrderByDescending(r => r.RegisteredAt)
            .Skip(page.Skip).Take(page.SafePageSize)
            .Select(MyRegistrationProjection)
            .ToListAsync(ct);

        var items = await AttachAttendanceCountsAsync(studentId, regs, ct);
        return new PagedResult<MyEventRegistrationDto>(items, page.SafePage, page.SafePageSize, total);
    }

    // Single-event lookup for the event detail page. Paging the inbox list above must not break
    // "am I registered for THIS event": a user whose registration fell off page 1 would otherwise
    // see a false "not registered" state and could try to register again.
    public async Task<MyEventRegistrationDto?> GetMyRegistrationForEventAsync(
        int eventId, int studentId, CancellationToken ct = default)
    {
        var reg = await db.EventRegistrations.AsNoTracking()
            .Where(r => r.StudentId == studentId && r.EventId == eventId && r.Status != RegistrationStatus.Cancelled)
            .Select(MyRegistrationProjection)
            .FirstOrDefaultAsync(ct);

        if (reg is null)
            return null;

        var items = await AttachAttendanceCountsAsync(studentId, [reg], ct);
        return items[0];
    }

    private static readonly Expression<Func<EventRegistration, MyRegistrationRow>> MyRegistrationProjection =
        r => new MyRegistrationRow(
            r.EventId,
            r.Event.Title,
            r.Event.OrganizationId,
            r.Event.Organization.Name,
            r.Event.Type,
            r.Event.DeliveryMode,
            r.Event.Location,
            r.Event.StartDate,
            r.Event.EndDate,
            r.Status,
            r.RegisteredAt,
            r.Event.Sessions.Count());

    private sealed record MyRegistrationRow(
        int EventId,
        string Title,
        int OrganizationId,
        string OrganizationName,
        EventType Type,
        EventDeliveryMode DeliveryMode,
        string Location,
        DateTime StartDate,
        DateTime EndDate,
        RegistrationStatus Status,
        DateTime RegisteredAt,
        int TotalSessions);

    private async Task<List<MyEventRegistrationDto>> AttachAttendanceCountsAsync(
        int studentId, IReadOnlyCollection<MyRegistrationRow> regs, CancellationToken ct)
    {
        if (regs.Count == 0)
            return [];

        var eventIds = regs.Select(r => r.EventId).ToArray();
        var attendedByEvent = await db.SessionAttendances.AsNoTracking()
            .Where(a => a.StudentId == studentId && eventIds.Contains(a.Session.EventId))
            .GroupBy(a => a.Session.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count, ct);

        return regs.Select(r => new MyEventRegistrationDto(
            r.EventId,
            r.Title,
            r.OrganizationId,
            r.OrganizationName,
            r.Type,
            r.DeliveryMode,
            r.Location,
            r.StartDate,
            r.EndDate,
            r.Status,
            r.RegisteredAt,
            attendedByEvent.GetValueOrDefault(r.EventId),
            r.TotalSessions)).ToList();
    }

    public async Task<IReadOnlyList<EventRegistrationDto>> GetRegistrationsAsync(
        int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageEventAsync(eventId, actorStudentId, actorRole, ct);

        return await db.EventRegistrations.AsNoTracking()
            .Where(x => x.EventId == eventId && x.Status != RegistrationStatus.Cancelled)
            .OrderBy(x => x.Status).ThenBy(x => x.RegisteredAt)
            .Select(x => new EventRegistrationDto(x.EventId, x.StudentId,
                x.Student.FirstName, x.Student.LastName, x.Student.Email, x.RegisteredAt, x.Status))
            .ToListAsync(ct);
    }

    public async Task<EventRegistrationDto> RegisterAsync(int eventId, int studentId, CancellationToken ct = default)
    {
        return await db.ExecuteInTransactionAsync(IsolationLevel.Serializable, async token =>
        {
            var eventEntity = await LoadEventWithRowLockAsync(eventId, token)
                ?? throw new NotFoundException($"Event with ID {eventId} was not found.");
            // Business rule: registration closes when the event is over. Before this check a student
            // could claim a confirmed seat (and later attendance XP) in an event that already ended.
            if (eventEntity.EndDate <= _time.GetUtcNow().UtcDateTime)
                throw new ConflictException("Registration is closed because this event has already ended.");
            var student = await db.Students.FirstOrDefaultAsync(x => x.StudentId == studentId, token)
                ?? throw new NotFoundException($"Student with ID {studentId} was not found.");
            var existing = await db.EventRegistrations.IgnoreQueryFilters([QueryFilterNames.SoftDelete]).FirstOrDefaultAsync(
                x => x.EventId == eventId && x.StudentId == studentId, token);
            if (existing is not null && existing.DeletedAt is null && existing.Status != RegistrationStatus.Cancelled)
                throw new ConflictException("You already have an active registration for this event.");

            var confirmedCount = await db.EventRegistrations.CountAsync(
                x => x.EventId == eventId && x.Status == RegistrationStatus.Confirmed, token);
            var status = confirmedCount < eventEntity.Capacity
                ? RegistrationStatus.Confirmed
                : RegistrationStatus.Waitlisted;

            if (existing is null)
            {
                existing = new EventRegistration
                {
                    EventId = eventId,
                    StudentId = studentId,
                    RegisteredAt = _time.GetUtcNow().UtcDateTime,
                    Status = status
                };
                db.EventRegistrations.Add(existing);
            }
            else
            {
                existing.DeletedAt = null;
                existing.RegisteredAt = _time.GetUtcNow().UtcDateTime;
                existing.Status = status;
            }

            existing.AddDomainEvent(new EventRegisteredDomainEvent(eventId, studentId, eventEntity.Title, status));
            await db.SaveChangesAsync(token);
            return new EventRegistrationDto(eventId, studentId, student.FirstName, student.LastName,
                student.Email, existing.RegisteredAt, existing.Status);
        }, ct);
    }

    public async Task CancelRegistrationAsync(
        int eventId, int targetStudentId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        if (targetStudentId != actorStudentId)
            await orgAuth.EnsureCanManageEventAsync(eventId, actorStudentId, actorRole, ct);

        // STRIDE THR-D-02: Serializable transaction + explicit SQL Server UPDLOCK/ROWLOCK preventing waitlist race conditions
        await db.ExecuteInTransactionAsync(IsolationLevel.Serializable, async token =>
        {
            var eventEntity = await LoadEventWithRowLockAsync(eventId, token)
                ?? throw new NotFoundException($"Event with ID {eventId} was not found.");

            var registration = await db.EventRegistrations.FirstOrDefaultAsync(
                x => x.EventId == eventId && x.StudentId == targetStudentId, token)
                ?? throw new NotFoundException("Event registration was not found.");
            if (registration.Status == RegistrationStatus.Cancelled)
                throw new ConflictException("This event registration has already been cancelled.");

            var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;
            registration.Status = RegistrationStatus.Cancelled;
            registration.DeletedAt = _time.GetUtcNow().UtcDateTime;
            if (wasConfirmed)
            {
                var next = await db.EventRegistrations
                    .Where(x => x.EventId == eventId && x.Status == RegistrationStatus.Waitlisted)
                    .OrderBy(x => x.RegisteredAt)
                    .FirstOrDefaultAsync(token);
                if (next is not null)
                {
                    next.Status = RegistrationStatus.Confirmed;
                    next.AddDomainEvent(new WaitlistPromotedDomainEvent(eventId, next.StudentId, eventEntity.Title));
                }
            }
            await db.SaveChangesAsync(token);
        }, ct);
    }

    #endregion

    #region Attendance & Check-In

    public async Task<IReadOnlyList<SessionAttendanceDto>> GetAttendanceAsync(
        int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

        return await db.SessionAttendances.AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CheckInTime)
            .Select(x => new SessionAttendanceDto(
                x.SessionId,
                x.StudentId,
                x.Student.FirstName,
                x.Student.LastName,
                x.CheckInTime,
                x.VerificationMethod,
                x.RecordedByStudentId))
            .ToListAsync(ct);
    }

    public async Task<SessionCheckInCodeDto> GenerateCheckInCodeAsync(
        int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);
        var (code, expiresAt) = attendanceCodes.GenerateCode(sessionId);
        return new SessionCheckInCodeDto(sessionId, code, expiresAt);
    }

    public async Task<SessionAttendanceDto> CheckInAsync(
        int sessionId, int actorStudentId, string? actorRole, CreateAttendanceRequest request, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

        if (!Enum.IsDefined(request.VerificationMethod))
            throw new BadRequestException("Invalid attendance verification method.");

        // STRIDE THR-T-02: Validate dynamic rotating check-in code whenever supplied for QR check-in
        if (request.VerificationMethod == VerificationMethod.QRCode
            && !string.IsNullOrWhiteSpace(request.VerificationCode)
            && !attendanceCodes.ValidateCode(sessionId, request.VerificationCode))
        {
            throw new BadRequestException("Invalid or expired session QR check-in code.");
        }

        // An organizer "QR" check-in without a code proves nothing; record it truthfully as a manual check-in (TU-SEC-010).
        var method = request.VerificationMethod == VerificationMethod.QRCode && string.IsNullOrWhiteSpace(request.VerificationCode)
            ? VerificationMethod.ManualByOrganizer
            : request.VerificationMethod;

        return await RecordAttendanceCoreAsync(
            sessionId,
            request.StudentId,
            method,
            recordedByStudentId: actorStudentId > 0 ? actorStudentId : null,
            ct);
    }

    public async Task<SessionAttendanceDto> SelfCheckInAsync(
        int sessionId, int studentId, string verificationCode, CancellationToken ct = default)
    {
        if (!await db.EventSessions.AsNoTracking().AnyAsync(s => s.SessionId == sessionId, ct))
            throw new NotFoundException($"Session with ID {sessionId} was not found.");

        if (!attendanceCodes.ValidateCode(sessionId, verificationCode))
            throw new BadRequestException("Invalid or expired session check-in code.");

        // Detective control for a relayed code: cap self check-ins per code window (revoked rows still count).
        var windowStart = _time.GetUtcNow().UtcDateTime.AddSeconds(-AttendanceCodeService.WindowSeconds);
        var recentSelfCheckIns = await db.SessionAttendances.IgnoreQueryFilters([QueryFilterNames.SoftDelete]).CountAsync(
            a => a.SessionId == sessionId && a.RecordedByStudentId == a.StudentId && a.CheckInTime >= windowStart, ct);
        if (recentSelfCheckIns >= attendanceOptions.Value.MaxSelfCheckInsPerWindow)
            throw new ConflictException("Too many check-ins for this session right now. Ask the organizer to verify you manually.");

        return await RecordAttendanceCoreAsync(
            sessionId,
            studentId,
            VerificationMethod.QRCode,
            recordedByStudentId: studentId,
            ct);
    }

    public async Task RevokeAttendanceAsync(
        int sessionId, int studentId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

        var attendance = await db.SessionAttendances.FirstOrDefaultAsync(
            x => x.SessionId == sessionId && x.StudentId == studentId, ct)
            ?? throw new NotFoundException("Session attendance record was not found.");
        db.SessionAttendances.Remove(attendance);

        // Integrity: revoking attendance must claw back the XP the check-in awarded, otherwise a student keeps points
        // for a session they never attended. ScoreTransaction is an append-only ledger (see SoftDeleteInterceptor), so
        // the claw-back is a compensating negative row, not a delete. Its (StudentId, SourceType, SourceReferenceId)
        // unique slot also collapses a concurrent double-revoke into a single compensation.
        var awardedPoints = await db.ScoreTransactions.AsNoTracking()
            .Where(t => t.StudentId == studentId
                && t.SourceType == ScoreSourceType.EventAttendance
                && t.SourceReferenceId == sessionId)
            .Select(t => (int?)t.Points)
            .FirstOrDefaultAsync(ct);
        if (awardedPoints is > 0)
        {
            db.ScoreTransactions.Add(new ScoreTransaction
            {
                StudentId = studentId,
                Points = -awardedPoints.Value,
                Reason = $"Revoked attendance for session #{sessionId}",
                SourceType = ScoreSourceType.AttendanceRevocation,
                SourceReferenceId = sessionId,
                CreatedAt = _time.GetUtcNow().UtcDateTime
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<string> RenderAttendanceReportHtmlAsync(
        int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        await orgAuth.EnsureCanManageSessionAsync(sessionId, actorStudentId, actorRole, ct);

        var session = await db.EventSessions.AsNoTracking()
            .Include(s => s.Event)
            .ThenInclude(e => e.Organization)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, ct)
            ?? throw new NotFoundException($"Session with ID {sessionId} was not found.");

        var attendees = await db.SessionAttendances.AsNoTracking()
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.CheckInTime)
            .Select(a => new
            {
                a.StudentId,
                FullName = a.Student.FirstName + " " + a.Student.LastName,
                a.VerificationMethod,
                a.CheckInTime,
                a.RecordedByStudentId
            })
            .ToListAsync(ct);

        var confirmedCount = await db.EventRegistrations.AsNoTracking()
            .CountAsync(r => r.EventId == session.EventId && r.Status == RegistrationStatus.Confirmed, ct);

        var rows = new StringBuilder();
        if (attendees.Count == 0)
        {
            rows.Append("<tr><td colspan=\"5\">No attendance records yet.</td></tr>");
        }
        else
        {
            foreach (var row in attendees)
            {
                rows.Append("<tr>")
                    .Append($"<td>{row.StudentId}</td>")
                    .Append($"<td>{WebUtility.HtmlEncode(row.FullName)}</td>")
                    .Append($"<td>{WebUtility.HtmlEncode(row.VerificationMethod.ToString())}</td>")
                    .Append($"<td>{WebUtility.HtmlEncode(row.CheckInTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"))}</td>")
                    .Append($"<td>{(row.RecordedByStudentId.HasValue ? row.RecordedByStudentId.Value.ToString() : "—")}</td>")
                    .Append("</tr>");
            }
        }

        var values = new Dictionary<string, string>
        {
            ["OrganizationName"] = WebUtility.HtmlEncode(session.Event.Organization.Name),
            ["EventTitle"] = WebUtility.HtmlEncode(session.Event.Title),
            ["SessionTitle"] = WebUtility.HtmlEncode($"#{session.SessionNo} — {session.Title}"),
            ["Room"] = WebUtility.HtmlEncode(session.Room ?? session.Event.Location),
            ["SessionWindow"] = WebUtility.HtmlEncode($"{session.StartTime:yyyy-MM-dd HH:mm} – {session.EndTime:HH:mm} UTC"),
            ["CheckedInCount"] = attendees.Count.ToString(),
            ["ConfirmedCount"] = confirmedCount.ToString(),
            ["GeneratedAtUtc"] = WebUtility.HtmlEncode(_time.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")),
            ["AttendanceRows"] = rows.ToString()
        };

        return await templateRenderer.RenderReportAsync("SessionAttendanceReport.html", values, ct);
    }

    public async Task<string> ExportAttendanceCsvAsync(
        int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var records = await GetAttendanceAsync(sessionId, actorStudentId, actorRole, ct);
        var headers = new[] { "SessionId", "StudentId", "FullName", "CheckInTimeUtc", "VerificationMethod", "RecordedByStudentId" };
        var rows = records.Select(r => new[]
        {
            r.SessionId.ToString(),
            r.StudentId.ToString(),
            $"{r.FirstName} {r.LastName}",
            r.CheckInTime.ToString("yyyy-MM-dd HH:mm:ss"),
            r.VerificationMethod.ToString(),
            r.RecordedByStudentId?.ToString() ?? "Organizer"
        });
        return CsvExportHelper.BuildCsv(headers, rows);
    }

    private async Task<SessionAttendanceDto> RecordAttendanceCoreAsync(
        int sessionId,
        int studentId,
        VerificationMethod verificationMethod,
        int? recordedByStudentId,
        CancellationToken ct)
    {
        var session = await db.EventSessions.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct)
            ?? throw new NotFoundException($"Session with ID {sessionId} was not found.");
        var student = await db.Students.FirstOrDefaultAsync(x => x.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");
        var isRegistered = await db.EventRegistrations.AnyAsync(x => x.EventId == session.EventId
            && x.StudentId == studentId && x.Status == RegistrationStatus.Confirmed, ct);
        if (!isRegistered)
            throw new BadRequestException("Only students with a confirmed event registration can check in.");
        if (await db.SessionAttendances.IgnoreQueryFilters([QueryFilterNames.SoftDelete]).AnyAsync(x => x.SessionId == sessionId && x.StudentId == studentId, ct))
            throw new ConflictException("The student has already checked in to this session.");

        // Check-in is only meaningful while the session is actually happening. Without this window a student can claim
        // attendance (and its XP) days before or after the session. The margin lets organizers open the room a little
        // early and record stragglers just after the end; both bounds are configurable via Attendance options.
        var now = _time.GetUtcNow().UtcDateTime;
        var earliest = session.StartTime.AddMinutes(-attendanceOptions.Value.EarlyCheckInMinutes);
        var latest = session.EndTime.AddMinutes(attendanceOptions.Value.LateCheckInMinutes);
        if (now < earliest || now > latest)
            throw new BadRequestException(
                $"Check-in for this session is only open from {earliest:yyyy-MM-dd HH:mm} to {latest:yyyy-MM-dd HH:mm} UTC.");

        // Ensure RecordedByStudentId satisfies FK if provided
        int? safeRecorderId = null;
        if (recordedByStudentId is > 0 && await db.Students.AnyAsync(s => s.StudentId == recordedByStudentId.Value, ct))
            safeRecorderId = recordedByStudentId.Value;

        var attendance = new SessionAttendance
        {
            SessionId = sessionId,
            StudentId = studentId,
            CheckInTime = now,
            VerificationMethod = verificationMethod,
            RecordedByStudentId = safeRecorderId
        };
        attendance.AddDomainEvent(new SessionAttendedDomainEvent(sessionId, session.EventId, studentId, session.Title));

        db.SessionAttendances.Add(attendance);
        await db.SaveChangesAsync(ct);

        return new SessionAttendanceDto(
            sessionId,
            studentId,
            student.FirstName,
            student.LastName,
            attendance.CheckInTime,
            attendance.VerificationMethod,
            attendance.RecordedByStudentId);
    }

    #endregion

    #region Helpers & Mapping

    private async Task<Event?> LoadEventWithRowLockAsync(int eventId, CancellationToken ct)
    {
        if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await db.Events
                .FromSql($"SELECT * FROM [Event] WITH (UPDLOCK, ROWLOCK) WHERE [EventId] = {eventId} AND [DeletedAt] IS NULL")
                .FirstOrDefaultAsync(ct);
        }

        return await db.Events.FirstOrDefaultAsync(x => x.EventId == eventId, ct);
    }

    private static void ValidateSessionWindow(Event parent, DateTime start, DateTime end)
    {
        if (end < start)
            throw new BadRequestException("Session EndTime must be greater than or equal to StartTime.");
        if (start < parent.StartDate || end > parent.EndDate)
            throw new BadRequestException("Session time must be within the parent event date range.");
    }

    private static EventDto ToEventDto(Event entity, int registeredCount, bool exposeMeetingUrl) => new(
        entity.EventId,
        entity.Title,
        entity.Description,
        entity.Type,
        entity.Location,
        entity.StartDate,
        entity.EndDate,
        entity.Capacity,
        entity.OrganizationId,
        entity.Organization.Name,
        registeredCount,
        entity.Sessions.OrderBy(x => x.SessionNo).Select(ToSessionDto).ToList(),
        entity.DeliveryMode,
        entity.IsPublic,
        exposeMeetingUrl ? entity.MeetingUrl : null,
        entity.OrganizationUnitId,
        entity.Unit?.Name,
        entity.CoverImageUrl ?? DefaultCoverCatalog.UrlFor($"event:{entity.EventId}"));

    private static EventSessionDto ToSessionDto(EventSession session) => new(
        session.SessionId, session.EventId, session.SessionNo, session.Title,
        session.StartTime, session.EndTime, session.Room);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #endregion
}
