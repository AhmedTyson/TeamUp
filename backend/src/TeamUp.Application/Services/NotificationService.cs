using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class NotificationService(
    IAppDbContext db,
    IOptions<NotificationOptions>? notificationOptions = null,
    TimeProvider? timeProvider = null) : INotificationService
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int studentId, bool unreadOnly = false, int? take = null, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var query = db.Notifications.AsNoTracking().Where(n => n.StudentId == studentId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        // Cap the inbox: an unbounded ToListAsync() here grows forever with the account age.
        // Default and maximum page sizes come from the Notifications configuration section.
        var options = notificationOptions?.Value ?? new NotificationOptions();
        var maxTake = Math.Max(options.MaxInboxTake, 1);
        var defaultTake = Math.Clamp(options.DefaultInboxTake, 1, maxTake);
        var effectiveTake = Math.Clamp(take ?? defaultTake, 1, maxTake);
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(effectiveTake)
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

    public async Task<int> GetUnreadCountAsync(int studentId, CancellationToken ct = default)
    {
        return await db.Notifications.AsNoTracking()
            .CountAsync(n => n.StudentId == studentId && !n.IsRead, ct);
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
        // One UPDATE statement instead of loading every unread row and saving them one by one.
        await db.Notifications
            .Where(n => n.StudentId == studentId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
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
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }
}
