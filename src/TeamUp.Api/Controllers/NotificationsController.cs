using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Endpoint #92: Retrieve authenticated student's notifications
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var studentId = User.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return Unauthorized();

        var result = await _notificationService.GetStudentNotificationsAsync(studentId, unreadOnly, page, limit);
        return Ok(result);
    }

    // Endpoint #94: Mark all own notifications as read
    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var studentId = User.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(studentId);
        return Ok(new { message = $"{count} notifications marked as read" });
    }

    // Endpoint #93: Update notification read status
    [HttpPatch("{id}")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var studentId = User.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(id, studentId);
        if (!success) return NotFound(new { message = $"Notification {id} not found or already read" });

        return Ok(new { message = "Notification marked as read" });
    }

    // Endpoint #95: Soft-delete notification
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        var studentId = User.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return Unauthorized();

        var success = await _notificationService.SoftDeleteAsync(id, studentId);
        if (!success) return NotFound(new { message = $"Notification {id} not found" });

        return Ok(new { message = "Notification soft-deleted successfully" });
    }
}

