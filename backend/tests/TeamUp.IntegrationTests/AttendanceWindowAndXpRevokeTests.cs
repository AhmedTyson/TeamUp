// Date: 2026-11-25
// Purpose: Regression tests for the attendance session window (check-in rejected before/after the session)
// and for XP claw-back when attendance is revoked (plus clean re-award on a later legitimate re-check-in).

using System.Net;
using System.Net.Http.Json;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class AttendanceWindowAndXpRevokeTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private async Task<(HttpClient OrgAdmin, EventSessionDto Session, int EventId)> CreateEventWithSessionAsync(
        string name, DateTime sessionStart, DateTime sessionEnd)
    {
        var orgAdmin = _api.Authed(factory.CreateToken(
            Random.Shared.Next(100000, 999999), $"orgadmin_{Guid.NewGuid():N}@teamup.test", "OrgAdmin"));
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(await orgAdmin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"{name}_{Guid.NewGuid():N}", "org", "Technology", null), ApiHelper.Json));
        // Event stays open-ended enough that registration remains allowed even for past-session
        // fixtures: registration now closes at event end, while check-in windows are per-session.
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest(name, "d", EventType.Workshop, "Hall",
                sessionStart.AddHours(-1), sessionEnd.AddHours(25), 10, org.OrganizationId), ApiHelper.Json));
        var session = await ApiHelper.DataAsync<EventSessionDto>(await orgAdmin.PostAsJsonAsync(
            $"/api/v1/events/{ev.EventId}/sessions",
            new CreateEventSessionRequest(1, "S1", sessionStart, sessionEnd, "A"), ApiHelper.Json));
        return (orgAdmin, session, ev.EventId);
    }

    private async Task<HttpClient> RegisterAndJoinEventAsync(string prefix, int eventId)
    {
        var (client, _, _, _) = await _api.RegisterAsync(prefix);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsync($"/api/v1/events/{eventId}/registrations", null)).StatusCode);
        return client;
    }

    [Fact]
    public async Task CheckIn_BeforeSessionWindow_IsRejected()
    {
        var (orgAdmin, session, eventId) = await CreateEventWithSessionAsync(
            "EarlyWindow", DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(3));
        var (studentClient, studentId, _, _) = await _api.RegisterAsync("earlybird");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{eventId}/registrations", null)).StatusCode);

        var res = await orgAdmin.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance",
            new CreateAttendanceRequest(studentId, VerificationMethod.ManualByOrganizer), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CheckIn_AfterSessionWindow_IsRejected()
    {
        var (orgAdmin, session, eventId) = await CreateEventWithSessionAsync(
            "LateWindow", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2));
        var (studentClient, studentId, _, _) = await _api.RegisterAsync("latecomer");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{eventId}/registrations", null)).StatusCode);

        var res = await orgAdmin.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance",
            new CreateAttendanceRequest(studentId, VerificationMethod.ManualByOrganizer), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CheckIn_WithinSessionWindow_StillWorks()
    {
        var (orgAdmin, session, eventId) = await CreateEventWithSessionAsync(
            "LiveWindow", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddHours(2));
        var (studentClient, studentId, _, _) = await _api.RegisterAsync("ontime");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{eventId}/registrations", null)).StatusCode);

        var res = await orgAdmin.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance",
            new CreateAttendanceRequest(studentId, VerificationMethod.ManualByOrganizer), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task RevokeAttendance_ClawBacksXp_AndReCheckIn_ReAwards()
    {
        var (orgAdmin, session, eventId) = await CreateEventWithSessionAsync(
            "XpRevoke", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddHours(2));
        var (studentClient, studentId, _, _) = await _api.RegisterAsync("xpclaw");
        Assert.Equal(HttpStatusCode.Created,
            (await studentClient.PostAsync($"/api/v1/events/{eventId}/registrations", null)).StatusCode);

        var checkIn = new CreateAttendanceRequest(studentId, VerificationMethod.ManualByOrganizer);
        Assert.Equal(HttpStatusCode.Created, (await orgAdmin.PostAsJsonAsync(
            $"/api/v1/sessions/{session.SessionId}/attendance", checkIn, ApiHelper.Json)).StatusCode);
        Assert.Equal(25, await ApiHelper.ScoreAsync(studentClient, studentId));

        Assert.Equal(HttpStatusCode.NoContent,
            (await orgAdmin.DeleteAsync($"/api/v1/sessions/{session.SessionId}/attendance/{studentId}")).StatusCode);
        Assert.Equal(0, await ApiHelper.ScoreAsync(studentClient, studentId));

        // Revocation is terminal: the soft-deleted attendance row still blocks re-check-in, so the clawed-back XP
        // cannot be farmed again by checking in repeatedly.
        Assert.Equal(HttpStatusCode.Conflict, (await orgAdmin.PostAsJsonAsync(
            $"/api/v1/sessions/{session.SessionId}/attendance", checkIn, ApiHelper.Json)).StatusCode);
        Assert.Equal(0, await ApiHelper.ScoreAsync(studentClient, studentId));
    }
}
