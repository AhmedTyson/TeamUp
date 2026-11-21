using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/events")]
[Authorize]
public sealed class EventsController(IEventService eventService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<EventDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? search, [FromQuery] int? organizationId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await eventService.GetEventsAsync(search, organizationId, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<EventDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var result = await eventService.CreateEventAsync(request, ct);
        return CreatedAtAction(nameof(GetEventById), new { id = result.EventId },
            BaseResponse<EventDto>.Ok(result, "Event created successfully.", StatusCodes.Status201Created));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventById([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetEventByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEvent([FromRoute] int id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        var result = await eventService.UpdateEventAsync(id, request, ct);
        return OkResponse(result, "Event updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteEvent([FromRoute] int id, CancellationToken ct)
    {
        await eventService.DeleteEventAsync(id, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/sessions")]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<EventSessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetSessionsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/sessions")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<EventSessionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSession([FromRoute] int id, [FromBody] CreateEventSessionRequest request, CancellationToken ct)
    {
        var result = await eventService.CreateSessionAsync(id, request, ct);
        return CreatedResponse(result, "Event session created successfully.");
    }
}
