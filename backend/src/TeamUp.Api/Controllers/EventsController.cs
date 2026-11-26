// Date: 2026-11-21
// Purpose: Campus events & session scheduling endpoints with scoped organization BOLA authorization and type/date filtering.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/events")]
[Authorize]
public sealed class EventsController(IEventService eventService) : BaseApiController
{
    #region Public Discovery

    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesOk<PagedResult<EventDto>>]
    public async Task<IActionResult> GetPublicEvents(
        [FromQuery] EventFilterQuery filter,
        CancellationToken ct = default)
    {
        var result = await eventService.GetPublicEventsAsync(
            filter.Search, filter.OrganizationId, filter.ToPageRequest(), filter.Type, filter.From, filter.To, filter.DeliveryMode, ct);
        return OkResponse(result);
    }

    [HttpGet("public/{id:int}")]
    [AllowAnonymous]
    [ProducesOk<EventDto>, ProducesNotFound]
    public async Task<IActionResult> GetPublicEventById([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetPublicEventByIdAsync(id, ct);
        return OkResponse(result);
    }

    #endregion

    #region Event Management

    [HttpGet("my-registrations")]
    [ProducesOk<PagedResult<MyEventRegistrationDto>>]
    public async Task<IActionResult> GetMyRegistrations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await eventService.GetMyRegistrationsAsync(CurrentStudentId, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpGet]
    [ProducesOk<PagedResult<EventDto>>]
    public async Task<IActionResult> GetEvents(
        [FromQuery] EventFilterQuery filter,
        CancellationToken ct = default)
    {
        var result = await eventService.GetEventsAsync(
            filter.Search, filter.OrganizationId, filter.ToPageRequest(), filter.Type, filter.From, filter.To, filter.DeliveryMode, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesCreated<EventDto>, ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var result = await eventService.CreateEventAsync(CurrentStudentId, CurrentUserRole, request, ct);
        return CreatedResponse(nameof(GetEventById), new { id = result.EventId }, result, "Event created successfully.");
    }

    [HttpGet("{id:int}")]
    [ProducesOk<EventDto>, ProducesNotFound]
    public async Task<IActionResult> GetEventById([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetEventByIdAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesOk<EventDto>, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> UpdateEvent([FromRoute] int id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        var result = await eventService.UpdateEventAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Event updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteEvent([FromRoute] int id, CancellationToken ct)
    {
        await eventService.DeleteEventAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }

    #endregion

    #region Sessions

    [HttpGet("{id:int}/sessions")]
    [ProducesOk<IReadOnlyList<EventSessionDto>>, ProducesNotFound]
    public async Task<IActionResult> GetSessions([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetSessionsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/sessions")]
    [ProducesCreated<EventSessionDto>, ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> CreateSession([FromRoute] int id, [FromBody] CreateEventSessionRequest request, CancellationToken ct)
    {
        var result = await eventService.CreateSessionAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        return CreatedResponse(result, "Event session created successfully.");
    }

    #endregion
}
