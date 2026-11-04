using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface IAttendanceService
{
    Task<SessionAttendanceListResponseDto> ListSessionAttendanceAsync(string sessionId, int page, int limit);
    Task<SessionAttendanceResponseDto> VerifyAttendanceAsync(string sessionId, string studentId, string verificationMethod);
    Task<SessionAttendanceResponseDto> RevokeAttendanceAsync(string sessionId, string studentId);
}

public class AttendanceService : IAttendanceService
{
    private readonly TeamUpDbContext _db;
    private readonly IBadgeService _badgeService;
    private readonly INotificationService _notificationService;

    public AttendanceService(
        TeamUpDbContext db,
        IBadgeService badgeService,
        INotificationService notificationService)
    {
        _db = db;
        _badgeService = badgeService;
        _notificationService = notificationService;
    }

    public async Task<SessionAttendanceListResponseDto> ListSessionAttendanceAsync(string sessionId, int page, int limit)
    {
        var session = await _db.EventSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException($"Session {sessionId} not found");

        var query = _db.SessionAttendances
            .Include(a => a.Student)
            .AsNoTracking()
            .Where(a => a.SessionID == sessionId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CheckInTime)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(a => new SessionAttendanceResponseDto(
                a.AttendanceID,
                a.SessionID,
                a.StudentID,
                a.CheckInTime,
                a.VerificationMethod,
                a.IsRevoked,
                a.RevokedAt,
                null,
                $"{a.Student.FirstName} {a.Student.LastName}",
                a.Student.Email
            ))
            .ToListAsync();

        return new SessionAttendanceListResponseDto(items, totalCount, page, limit);
    }

    public async Task<SessionAttendanceResponseDto> VerifyAttendanceAsync(string sessionId, string studentId, string verificationMethod)
    {
        var session = await _db.EventSessions
            .Include(s => s.Event)
            .FirstOrDefaultAsync(s => s.SessionID == sessionId && !s.IsDeleted && !s.Event.IsDeleted)
            ?? throw new KeyNotFoundException($"Active session {sessionId} not found");

        // Rule: verify attendance only for students with Confirmed registration in parent Event
        var isConfirmed = await _db.EventRegistrations.AnyAsync(r => 
            r.EventID == session.EventID && 
            r.StudentID == studentId && 
            r.Status == "Confirmed");

        if (!isConfirmed)
        {
            throw new InvalidOperationException("Student does not have an active confirmed registration for the parent event");
        }

        // Rule: prevent duplicate check-ins
        var existing = await _db.SessionAttendances
            .FirstOrDefaultAsync(a => a.SessionID == sessionId && a.StudentID == studentId);

        var now = DateTime.UtcNow;
        SessionAttendance att;

        if (existing != null)
        {
            if (!existing.IsRevoked)
            {
                throw new InvalidOperationException("Student has already checked in to this session");
            }
            existing.IsRevoked = false;
            existing.RevokedAt = null;
            existing.CheckInTime = now;
            existing.VerificationMethod = verificationMethod;
            att = existing;
        }
        else
        {
            att = new SessionAttendance
            {
                SessionID = sessionId,
                StudentID = studentId,
                VerificationMethod = verificationMethod,
                CheckInTime = now,
                IsRevoked = false
            };
            _db.SessionAttendances.Add(att);
        }

        // Rule: award +100 points via immutable ScoreTransaction
        var tx = new ScoreTransaction
        {
            StudentID = studentId,
            Points = 100,
            Reason = $"Session Attendance: {session.Title}",
            SourceType = "SessionAttendance",
            SourceID = sessionId,
            CreatedAt = now
        };
        _db.ScoreTransactions.Add(tx);

        await _db.SaveChangesAsync();

        // Evaluate eligible badges
        await _badgeService.EvaluateStudentBadgesAsync(studentId);

        // Send single AttendanceVerified notification
        await _notificationService.SendNotificationAsync(
            studentId,
            "Attendance Verified",
            $"Attendance confirmed for '{session.Title}'. +100 points added to your score!",
            "AttendanceVerified",
            $"attendance_verified:{sessionId}:{studentId}:{now.Ticks}"
        );

        return new SessionAttendanceResponseDto(
            att.AttendanceID,
            att.SessionID,
            att.StudentID,
            att.CheckInTime,
            att.VerificationMethod,
            att.IsRevoked,
            att.RevokedAt,
            100
        );
    }

    public async Task<SessionAttendanceResponseDto> RevokeAttendanceAsync(string sessionId, string studentId)
    {
        var session = await _db.EventSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException($"Session {sessionId} not found");

        var att = await _db.SessionAttendances
            .FirstOrDefaultAsync(a => a.SessionID == sessionId && a.StudentID == studentId);

        if (att == null || att.IsRevoked)
        {
            throw new InvalidOperationException("Attendance record not found or already revoked");
        }

        var now = DateTime.UtcNow;
        att.IsRevoked = true;
        att.RevokedAt = now;

        // Rule: insert compensating -100 ScoreTransaction
        var tx = new ScoreTransaction
        {
            StudentID = studentId,
            Points = -100,
            Reason = $"Attendance Revoked: {session.Title}",
            SourceType = "SessionAttendanceRevocation",
            SourceID = sessionId,
            CreatedAt = now
        };
        _db.ScoreTransactions.Add(tx);

        await _db.SaveChangesAsync();

        return new SessionAttendanceResponseDto(
            att.AttendanceID,
            att.SessionID,
            att.StudentID,
            att.CheckInTime,
            att.VerificationMethod,
            att.IsRevoked,
            att.RevokedAt,
            -100
        );
    }
}

