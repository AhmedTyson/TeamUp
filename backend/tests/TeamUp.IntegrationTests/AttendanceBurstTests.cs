// Date: 2026-11-24
// Purpose: TU-SEC-006 & TU-SEC-010 mitigation tests: relayed rotating code burst cap and empty-code organizer QR downgrade.

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class AttendanceBurstTests(TeamUpApiFactory baseFactory) : IClassFixture<TeamUpApiFactory>
{
    [Fact]
    public async Task SharedCode_IsRejectedOnceBurstCapIsReached_ButWithinCapStillWorks()
    {
        await using var capped = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(
                new Dictionary<string, string?> { ["Attendance:MaxSelfCheckInsPerWindow"] = "2" })));
        var api = new ApiHelper(baseFactory);

        var orgAdmin = capped.CreateClient();
        orgAdmin.DefaultRequestHeaders.Authorization = new("Bearer", baseFactory.CreateToken(911, "orgadmin_burst@teamup.test", "OrgAdmin"));
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(await orgAdmin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"Burst_{Guid.NewGuid():N}", "org", "Technology", null), ApiHelper.Json));
        var start = DateTime.UtcNow.AddHours(-1);
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Burst Workshop", "d", EventType.Workshop, "Hall", start, start.AddDays(1), 10, org.OrganizationId), ApiHelper.Json));
        var session = await ApiHelper.DataAsync<EventSessionDto>(await orgAdmin.PostAsJsonAsync($"/api/v1/events/{ev.EventId}/sessions",
            new CreateEventSessionRequest(1, "Opening", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddHours(2), "A"), ApiHelper.Json));
        var clients = new List<HttpClient>();
        for (var i = 0; i < 3; i++)
        {
            var (_, _, _, auth) = await api.RegisterAsync($"burst{i}");
            var client = capped.CreateClient();
            client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsync($"/api/v1/events/{ev.EventId}/registrations", null)).StatusCode);
            clients.Add(client);
        }

        // Ensure all 3 rapid self check-ins fall inside the same 60s rotating-code step even across minute boundaries.
        if (DateTime.UtcNow.Second >= 57)
            await Task.Delay(3500);

        var code = await ApiHelper.DataAsync<SessionCheckInCodeDto>(
            await orgAdmin.PostAsync($"/api/v1/sessions/{session.SessionId}/attendance/check-in-code", null));

        var statuses = new List<HttpStatusCode>();
        foreach (var client in clients)
        {
            statuses.Add((await client.PostAsJsonAsync($"/api/v1/sessions/{session.SessionId}/attendance/self-check-in",
                new SelfCheckInRequest(code.Code), ApiHelper.Json)).StatusCode);
        }

        Assert.Equal(HttpStatusCode.Created, statuses[0]);
        Assert.Equal(HttpStatusCode.Created, statuses[1]);
        Assert.Equal(HttpStatusCode.Conflict, statuses[2]);
    }

    [Fact]
    public async Task OrganizerQrCheckInWithoutCode_IsRecordedAsManualByOrganizer()
    {
        var api = new ApiHelper(baseFactory);
        var orgAdmin = api.Authed(baseFactory.CreateToken(912, "orgadmin_manual@teamup.test", "OrgAdmin"));
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(await orgAdmin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"ManualQr_{Guid.NewGuid():N}", "org", "Technology", null), ApiHelper.Json));
        var start = DateTime.UtcNow.AddHours(-1);
        var ev = await ApiHelper.DataAsync<EventDto>(await orgAdmin.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Manual QR Audit", "d", EventType.Workshop, "Hall", start, start.AddDays(1), 10, org.OrganizationId), ApiHelper.Json));
        var session = await ApiHelper.DataAsync<EventSessionDto>(await orgAdmin.PostAsJsonAsync($"/api/v1/events/{ev.EventId}/sessions",
            new CreateEventSessionRequest(1, "Check-in", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddHours(2), "B"), ApiHelper.Json));

        var (studentClient, studentId, _, _) = await api.RegisterAsync("manualqr");
        Assert.Equal(HttpStatusCode.Created, (await studentClient.PostAsync($"/api/v1/events/{ev.EventId}/registrations", null)).StatusCode);

        var checkInRes = await orgAdmin.PostAsJsonAsync(
            $"/api/v1/sessions/{session.SessionId}/attendance",
            new CreateAttendanceRequest(studentId, VerificationMethod.QRCode),
            ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, checkInRes.StatusCode);

        var attendance = await ApiHelper.DataAsync<SessionAttendanceDto>(checkInRes);
        Assert.Equal(VerificationMethod.ManualByOrganizer, attendance.VerificationMethod);
    }
}
