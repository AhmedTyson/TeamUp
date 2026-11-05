using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Gamification;

namespace TeamUp.Api.Controllers;

[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : BaseApiController
{
    [HttpGet]
    [ProducesOk<IReadOnlyList<NotificationDto>>]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, CancellationToken ct = default)
    {
        var result = await notificationService.GetNotificationsAsync(CurrentStudentId, unreadOnly, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id, CancellationToken ct)
    {
        await notificationService.MarkAsReadAsync(CurrentStudentId, id, ct);
        return NoContentResponse();
    }

    [HttpPatch("mark-all-read")]
    [ProducesNoContent]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await notificationService.MarkAllAsReadAsync(CurrentStudentId, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> DeleteNotification([FromRoute] int id, CancellationToken ct)
    {
        await notificationService.DeleteNotificationAsync(CurrentStudentId, id, ct);
        return NoContentResponse();
    }
}
