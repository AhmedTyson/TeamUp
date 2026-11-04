using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface IEventService
{
    Task<EventListResponseDto> ListEventsAsync(string? eventType, string? organizationId, string? search, int page, int limit);
    Task<EventResponseDto> CreateEventAsync(EventCreateDto dto);
    Task<EventResponseDto?> GetEventAsync(string eventId);
    Task<EventResponseDto> UpdateEventAsync(string eventId, EventUpdateDto dto);
    Task<bool> SoftDeleteEventAsync(string eventId);
}

public class EventService : IEventService
{
    private readonly TeamUpDbContext _db;

    public EventService(TeamUpDbContext db)
    {
        _db = db;
    }

    public async Task<EventListResponseDto> ListEventsAsync(string? eventType, string? organizationId, string? search, int page, int limit)
    {
        var query = _db.Events
            .Include(e => e.Organization)
            .Include(e => e.Registrations)
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.Organization.IsActive);

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.Type == eventType);
        }

        if (!string.IsNullOrEmpty(organizationId))
        {
            query = query.Where(e => e.OrganizationID == organizationId);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search) || e.Location.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(e => new EventResponseDto(
                e.EventID,
                e.OrganizationID,
                e.Organization.Name,
                e.Title,
                e.Description,
                e.Type,
                e.Location,
                e.StartDate,
                e.EndDate,
                e.Capacity,
                e.Registrations.Count(r => r.Status == "Confirmed"),
                e.IsDeleted,
                e.CreatedAt,
                null
            ))
            .ToListAsync();

        return new EventListResponseDto(items, totalCount, page, limit);
    }

    public async Task<EventResponseDto> CreateEventAsync(EventCreateDto dto)
    {
        var org = await _db.Organizations.FindAsync(dto.OrganizationId)
            ?? throw new InvalidOperationException("Hosting organization does not exist or is inactive");

        if (!org.IsActive)
        {
            throw new InvalidOperationException("Hosting organization is inactive");
        }

        if (dto.EndDate <= dto.StartDate)
        {
            throw new ArgumentException("Event EndDate must be strictly after StartDate");
        }

        var newEvent = new Event
        {
            OrganizationID = dto.OrganizationId,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Capacity = dto.Capacity,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Sessions != null && dto.Sessions.Any())
        {
            foreach (var s in dto.Sessions)
            {
                if (s.EndTime <= s.StartTime)
                {
                    throw new ArgumentException("Session EndTime must be strictly after StartTime");
                }

                newEvent.Sessions.Add(new EventSession
                {
                    Title = s.Title,
                    Speaker = s.Speaker,
                    Room = s.Room,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsDeleted = false
                });
            }
        }

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();

        var sessionsDto = newEvent.Sessions.Select(s => new EventSessionResponseDto(
            s.SessionID,
            s.EventID,
            s.Title,
            s.Speaker,
            s.Room,
            s.StartTime,
            s.EndTime,
            s.IsDeleted
        )).ToList();

        return new EventResponseDto(
            newEvent.EventID,
            newEvent.OrganizationID,
            org.Name,
            newEvent.Title,
            newEvent.Description,
            newEvent.Type,
            newEvent.Location,
            newEvent.StartDate,
            newEvent.EndDate,
            newEvent.Capacity,
            0,
            newEvent.IsDeleted,
            newEvent.CreatedAt,
            sessionsDto
        );
    }

    public async Task<EventResponseDto?> GetEventAsync(string eventId)
    {
        var ev = await _db.Events
            .Include(e => e.Organization)
            .Include(e => e.Sessions.Where(s => !s.IsDeleted))
            .Include(e => e.Registrations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventID == eventId && !e.IsDeleted);

        if (ev == null) return null;

        var sessionsDto = ev.Sessions
            .OrderBy(s => s.StartTime)
            .Select(s => new EventSessionResponseDto(
                s.SessionID,
                s.EventID,
                s.Title,
                s.Speaker,
                s.Room,
                s.StartTime,
                s.EndTime,
                s.IsDeleted
            )).ToList();

        return new EventResponseDto(
            ev.EventID,
            ev.OrganizationID,
            ev.Organization.Name,
            ev.Title,
            ev.Description,
            ev.Type,
            ev.Location,
            ev.StartDate,
            ev.EndDate,
            ev.Capacity,
            ev.Registrations.Count(r => r.Status == "Confirmed"),
            ev.IsDeleted,
            ev.CreatedAt,
            sessionsDto
        );
    }

    public async Task<EventResponseDto> UpdateEventAsync(string eventId, EventUpdateDto dto)
    {
        var ev = await _db.Events
            .Include(e => e.Organization)
            .Include(e => e.Sessions)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.EventID == eventId && !e.IsDeleted)
            ?? throw new KeyNotFoundException($"Event {eventId} not found");

        if (dto.Capacity.HasValue)
        {
            var confirmedCount = ev.Registrations.Count(r => r.Status == "Confirmed");
            if (dto.Capacity.Value < confirmedCount)
            {
                throw new InvalidOperationException($"Cannot reduce capacity to {dto.Capacity.Value}: already has {confirmedCount} active confirmed registrations");
            }
            ev.Capacity = dto.Capacity.Value;
        }

        var newStart = dto.StartDate ?? ev.StartDate;
        var newEnd = dto.EndDate ?? ev.EndDate;
        if (newEnd <= newStart)
        {
            throw new ArgumentException("EndDate must be strictly after StartDate");
        }

        if (dto.Title != null) ev.Title = dto.Title;
        if (dto.Description != null) ev.Description = dto.Description;
        if (dto.Location != null) ev.Location = dto.Location;
        if (dto.StartDate.HasValue) ev.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) ev.EndDate = dto.EndDate.Value;

        await _db.SaveChangesAsync();

        return (await GetEventAsync(eventId))!;
    }

    public async Task<bool> SoftDeleteEventAsync(string eventId)
    {
        var ev = await _db.Events
            .Include(e => e.Sessions)
            .FirstOrDefaultAsync(e => e.EventID == eventId && !e.IsDeleted);

        if (ev == null) return false;

        ev.IsDeleted = true;
        foreach (var session in ev.Sessions)
        {
            session.IsDeleted = true;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}

