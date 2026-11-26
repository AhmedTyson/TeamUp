// Date: 2026-11-25
// Purpose: Regression tests for the GET-endpoint pagination audit: the notifications inbox defaults to
// the latest 15 with an exact unread-count endpoint, my-registrations is paged, and the per-event
// registrations/mine lookup keeps the event detail page's "am I registered" check correct.

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class PaginationAndInboxTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private async Task SeedNotificationsAsync(int studentId, int count, int unreadCount)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var i = 0; i < count; i++)
        {
            db.Notifications.Add(new Notification
            {
                StudentId = studentId,
                Title = $"n{i}",
                Message = $"message {i}",
                Type = NotificationType.EventReminder,
                IsRead = i >= unreadCount,
                CreatedAt = DateTime.UtcNow.AddMinutes(-(count - i)),
            });
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Notifications_DefaultTake_ReturnsLatest15NewestFirst()
    {
        var (client, studentId, _, _) = await _api.RegisterAsync("inboxdefault");
        await SeedNotificationsAsync(studentId, 20, unreadCount: 20);

        var res = await client.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var items = await ApiHelper.DataAsync<List<NotificationDto>>(res);

        Assert.Equal(15, items.Count);
        Assert.Equal("n19", items[0].Title);
        Assert.Equal("n5", items[^1].Title);
        Assert.True(items.Zip(items.Skip(1)).All(pair => pair.First.CreatedAt >= pair.Second.CreatedAt));
    }

    [Fact]
    public async Task Notifications_TakeParam_IsRespectedWithinClamp()
    {
        var (client, studentId, _, _) = await _api.RegisterAsync("inboxtake");
        await SeedNotificationsAsync(studentId, 20, unreadCount: 20);

        var five = await ApiHelper.DataAsync<List<NotificationDto>>(
            await client.GetAsync("/api/v1/notifications?take=5"));
        Assert.Equal(5, five.Count);
        Assert.Equal("n19", five[0].Title);

        var unreadOnly = await ApiHelper.DataAsync<List<NotificationDto>>(
            await client.GetAsync("/api/v1/notifications?unreadOnly=true"));
        Assert.Equal(15, unreadOnly.Count);
        Assert.All(unreadOnly, n => Assert.False(n.IsRead));
    }

    [Fact]
    public async Task Notifications_UnreadCount_IsExactBeyondTheInboxCap()
    {
        var (client, studentId, _, _) = await _api.RegisterAsync("inboxcount");
        await SeedNotificationsAsync(studentId, 20, unreadCount: 17);

        var count = await ApiHelper.DataAsync<int>(await client.GetAsync("/api/v1/notifications/unread-count"));
        Assert.Equal(17, count);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PatchAsync("/api/v1/notifications/mark-all-read", null)).StatusCode);

        var after = await ApiHelper.DataAsync<int>(await client.GetAsync("/api/v1/notifications/unread-count"));
        Assert.Equal(0, after);
    }

    private async Task<(HttpClient OrgAdmin, int OrgId)> CreateOrgAsync(string name)
    {
        var orgAdmin = _api.Authed(factory.CreateToken(
            Random.Shared.Next(100000, 999999), $"orgadmin_{Guid.NewGuid():N}@teamup.test", "OrgAdmin"));
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(await orgAdmin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"{name}_{Guid.NewGuid():N}", "org", "Technology", null), ApiHelper.Json));
        return (orgAdmin, org.OrganizationId);
    }

    private async Task<int> CreateEventAsync(HttpClient orgAdmin, int orgId, string title)
    {
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest(title, "d", EventType.Workshop, "Hall",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, orgId), ApiHelper.Json));
        return ev.EventId;
    }

    [Fact]
    public async Task MyRegistrations_ArePagedNewestFirst()
    {
        var (orgAdmin, orgId) = await CreateOrgAsync("PagedRegs");
        var olderEventId = await CreateEventAsync(orgAdmin, orgId, "Older");
        var newerEventId = await CreateEventAsync(orgAdmin, orgId, "Newer");

        var (studentClient, _, _, _) = await _api.RegisterAsync("pagedregs");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{olderEventId}/registrations", null)).StatusCode);
        await Task.Delay(20); // keep RegisteredAt ordering deterministic
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{newerEventId}/registrations", null)).StatusCode);

        var page1 = await ApiHelper.DataAsync<PagedResult<MyEventRegistrationDto>>(
            await studentClient.GetAsync("/api/v1/events/my-registrations?page=1&pageSize=1"));
        Assert.Equal(2, page1.TotalCount);
        Assert.Equal(2, page1.TotalPages);
        Assert.Single(page1.Items);
        Assert.Equal(newerEventId, page1.Items[0].EventId);

        var page2 = await ApiHelper.DataAsync<PagedResult<MyEventRegistrationDto>>(
            await studentClient.GetAsync("/api/v1/events/my-registrations?page=2&pageSize=1"));
        Assert.Single(page2.Items);
        Assert.Equal(olderEventId, page2.Items[0].EventId);
    }

    [Fact]
    public async Task RegistrationsMine_ReturnsOwnRegistrationOrNull()
    {
        var (orgAdmin, orgId) = await CreateOrgAsync("MineLookup");
        var registeredEventId = await CreateEventAsync(orgAdmin, orgId, "Joined");
        var otherEventId = await CreateEventAsync(orgAdmin, orgId, "NotJoined");

        var (studentClient, _, _, _) = await _api.RegisterAsync("minelookup");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{registeredEventId}/registrations", null)).StatusCode);

        var mine = await ApiHelper.DataAsync<MyEventRegistrationDto?>(
            await studentClient.GetAsync($"/api/v1/events/{registeredEventId}/registrations/mine"));
        Assert.NotNull(mine);
        Assert.Equal(registeredEventId, mine.EventId);
        Assert.Equal(RegistrationStatus.Confirmed, mine.Status);

        // DataAsync asserts non-null data, so read the envelope directly for the null case.
        var noneRes = await studentClient.GetAsync($"/api/v1/events/{otherEventId}/registrations/mine");
        Assert.Equal(HttpStatusCode.OK, noneRes.StatusCode);
        var noneEnvelope = await noneRes.Content.ReadFromJsonAsync<BaseResponse<MyEventRegistrationDto?>>(ApiHelper.Json);
        Assert.NotNull(noneEnvelope);
        Assert.True(noneEnvelope.Success);
        Assert.Null(noneEnvelope.Data);
    }
}
