using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface IRegistrationService
{
    Task<EventRegistrationListResponseDto> ListRegistrationsAsync(string eventId, string? status, int page, int limit);
    Task<EventRegistrationResponseDto> RegisterForEventAsync(string eventId, string studentId);
    Task<EventRegistrationResponseDto> CancelRegistrationAsync(string eventId, string studentId);
}

public class RegistrationService : IRegistrationService
{
    private readonly TeamUpDbContext _db;
    private readonly INotificationService _notificationService;

    public RegistrationService(TeamUpDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<EventRegistrationListResponseDto> ListRegistrationsAsync(string eventId, string? status, int page, int limit)
    {
        var query = _db.EventRegistrations
            .Include(r => r.Student)
            .AsNoTracking()
            .Where(r => r.EventID == eventId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.RegisteredAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => new EventRegistrationResponseDto(
                r.RegistrationID,
                r.EventID,
                r.StudentID,
                r.Status,
                r.RegisteredAt,
                r.CancelledAt,
                new RegistrationStudentSummaryDto(
                    r.StudentID,
                    r.Student.Email,
                    r.Student.FirstName,
                    r.Student.LastName
                )
            ))
            .ToListAsync();

        return new EventRegistrationListResponseDto(items, totalCount, page, limit);
    }

    public async Task<EventRegistrationResponseDto> RegisterForEventAsync(string eventId, string studentId)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId && !e.IsDeleted)
            ?? throw new KeyNotFoundException($"Event {eventId} not found");

        var existing = await _db.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventID == eventId && r.StudentID == studentId);

        if (existing != null && (existing.Status == "Confirmed" || existing.Status == "Waitlisted"))
        {
            throw new InvalidOperationException($"Student is already registered for this event with status '{existing.Status}'");
        }

        var confirmedCount = await _db.EventRegistrations
            .CountAsync(r => r.EventID == eventId && r.Status == "Confirmed");

        var now = DateTime.UtcNow;
        var assignedStatus = confirmedCount < ev.Capacity ? "Confirmed" : "Waitlisted";

        EventRegistration reg;
        if (existing != null)
        {
            existing.Status = assignedStatus;
            existing.RegisteredAt = now;
            existing.CancelledAt = null;
            reg = existing;
        }
        else
        {
            reg = new EventRegistration
            {
                EventID = eventId,
                StudentID = studentId,
                Status = assignedStatus,
                RegisteredAt = now
            };
            _db.EventRegistrations.Add(reg);
        }

        await _db.SaveChangesAsync();

        var msg = assignedStatus == "Confirmed"
            ? $"Your registration for '{ev.Title}' is confirmed."
            : $"The event '{ev.Title}' is currently full. You have been placed on the waitlist.";

        await _notificationService.SendNotificationAsync(
            studentId,
            $"Registration {assignedStatus}",
            msg,
            "EventRegistration",
            $"registration:{eventId}:{studentId}:{assignedStatus}"
        );

        return new EventRegistrationResponseDto(
            reg.RegistrationID,
            reg.EventID,
            reg.StudentID,
            reg.Status,
            reg.RegisteredAt,
            reg.CancelledAt
        );
    }

    public async Task<EventRegistrationResponseDto> CancelRegistrationAsync(string eventId, string studentId)
    {
        var reg = await _db.EventRegistrations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.EventID == eventId && r.StudentID == studentId && r.Status != "Cancelled")
            ?? throw new KeyNotFoundException($"Active registration for student {studentId} on event {eventId} not found");

        // Attendance guard: check if student has recorded attendance in any session of this event
        var hasAttended = await _db.SessionAttendances
            .Include(a => a.Session)
            .AnyAsync(a => a.Session.EventID == eventId && a.StudentID == studentId && !a.IsRevoked);

        if (hasAttended)
        {
            throw new InvalidOperationException("Cannot cancel registration: Student has already checked in to one or more event sessions");
        }

        var wasConfirmed = reg.Status == "Confirmed";
        var now = DateTime.UtcNow;

        reg.Status = "Cancelled";
        reg.CancelledAt = now;

        // Auto promote oldest Waitlisted student (FIFO)
        if (wasConfirmed)
        {
            var oldestWaitlisted = await _db.EventRegistrations
                .Where(r => r.EventID == eventId && r.Status == "Waitlisted")
                .OrderBy(r => r.RegisteredAt)
                .FirstOrDefaultAsync();

            if (oldestWaitlisted != null)
            {
                oldestWaitlisted.Status = "Confirmed";
                await _notificationService.SendNotificationAsync(
                    oldestWaitlisted.StudentID,
                    "Waitlist Promotion!",
                    $"A spot opened up! Your registration for '{reg.Event.Title}' has been promoted to Confirmed.",
                    "EventRegistration",
                    $"promoted:{eventId}:{oldestWaitlisted.StudentID}"
                );
            }
        }

        await _db.SaveChangesAsync();

        return new EventRegistrationResponseDto(
            reg.RegistrationID,
            reg.EventID,
            reg.StudentID,
            reg.Status,
            reg.RegisteredAt,
            reg.CancelledAt
        );
    }
}

