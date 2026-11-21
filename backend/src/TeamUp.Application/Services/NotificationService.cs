using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class NotificationService(IAppDbContext db) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int studentId, bool unreadOnly = false, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var query = db.Notifications.AsNoTracking().Where(n => n.StudentId == studentId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.NotificationId,
                n.StudentId,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(int studentId, int notificationId, CancellationToken ct = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Notification with ID {notificationId} was not found.");

        notification.IsRead = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(int studentId, CancellationToken ct = default)
    {
        var unread = await db.Notifications
            .Where(n => n.StudentId == studentId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteNotificationAsync(int studentId, int notificationId, CancellationToken ct = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Notification with ID {notificationId} was not found.");

        db.Notifications.Remove(notification);
        await db.SaveChangesAsync(ct);
    }

    public async Task CreateNotificationAsync(int studentId, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            StudentId = studentId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }
}
