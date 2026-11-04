using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface INotificationService
{
    Task<string?> SendNotificationAsync(string studentId, string title, string message, string notifType, string? idempotencyKey = null);
    Task<NotificationListResponseDto> GetStudentNotificationsAsync(string studentId, bool unreadOnly, int page, int limit);
    Task<bool> MarkAsReadAsync(string notificationId, string studentId);
    Task<int> MarkAllAsReadAsync(string studentId);
    Task<bool> SoftDeleteAsync(string notificationId, string studentId);
}

public class NotificationService : INotificationService
{
    private readonly TeamUpDbContext _db;

    public NotificationService(TeamUpDbContext db)
    {
        _db = db;
    }

    public async Task<string?> SendNotificationAsync(string studentId, string title, string message, string notifType, string? idempotencyKey = null)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await _db.Notifications.FirstOrDefaultAsync(n => n.IdempotencyKey == idempotencyKey);
            if (existing != null) return existing.NotificationID;
        }

        var notif = new Notification
        {
            StudentID = studentId,
            Title = title,
            Message = message,
            Type = notifType,
            IsRead = false,
            IsDeleted = false,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync();
        return notif.NotificationID;
    }

    public async Task<NotificationListResponseDto> GetStudentNotificationsAsync(string studentId, bool unreadOnly, int page, int limit)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.StudentID == studentId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync();
        var unreadCount = await _db.Notifications
            .CountAsync(n => n.StudentID == studentId && !n.IsDeleted && !n.IsRead);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(n => new NotificationResponseDto(
                n.NotificationID,
                n.StudentID,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.IsDeleted,
                n.CreatedAt
            ))
            .ToListAsync();

        return new NotificationListResponseDto(items, unreadCount, totalCount, page, limit);
    }

    public async Task<bool> MarkAsReadAsync(string notificationId, string studentId)
    {
        var notif = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.StudentID == studentId && !n.IsDeleted);

        if (notif == null) return false;

        notif.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string studentId)
    {
        var unread = await _db.Notifications
            .Where(n => n.StudentID == studentId && !n.IsDeleted && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        await _db.SaveChangesAsync();
        return unread.Count;
    }

    public async Task<bool> SoftDeleteAsync(string notificationId, string studentId)
    {
        var notif = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.StudentID == studentId);

        if (notif == null) return false;

        notif.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}

