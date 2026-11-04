using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Api.Auth;
using TeamUp.Api.Models.DTOs;
using Xunit;

namespace TeamUp.Tests;

public class EndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private void SetAuthHeader(string studentId, string email, string role)
    {
        var token = JwtTokenService.GenerateToken(studentId, email, role);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Full_Cross_Module_E2E_Workflow_Tests_T22_Through_T27()
    {
        var now = DateTime.UtcNow;

        // 0. Admin creates a badge that unlocks at 100 points (#87)
        SetAuthHeader("admin-master", "admin@teamup.edu", "PlatformAdmin");
        var badgeRes = await _client.PostAsJsonAsync("/api/v1/badges", new BadgeCreateDto(
            "C# Pioneer",
            "Reach 100 total points in the platform",
            "https://teamup.edu/badges/pioneer.png",
            "TotalPoints",
            100
        ));
        Assert.Equal(HttpStatusCode.Created, badgeRes.StatusCode);
        var badgeData = await badgeRes.Content.ReadFromJsonAsync<BadgeResponseDto>();
        Assert.NotNull(badgeData);

        // A. OrgAdmin Ibrahim creates an event (#69)
        SetAuthHeader("std-ibrahim", "ibrahim@teamup.edu", "Student");
        var eventPayload = new EventCreateDto(
            "org-ieee",
            "C# .NET 10 Microservices Workshop",
            "Complete hands-on with ASP.NET Core & clean architecture",
            "Workshop",
            "Hall A, Computer Science Building",
            now.AddDays(5),
            now.AddDays(5).AddHours(4),
            1, // Capacity = 1 to test waitlisting
            new List<EventSessionCreateDto>
            {
                new("Architecture & APIs", "Dr. Ahmed", "Room 101", now.AddDays(5), now.AddDays(5).AddHours(2))
            }
        );
        var createEventRes = await _client.PostAsJsonAsync("/api/v1/events", eventPayload);
        Assert.Equal(HttpStatusCode.Created, createEventRes.StatusCode);
        var createdEvent = await createEventRes.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.NotNull(createdEvent);
        var eventId = createdEvent.EventId;
        var sessionId = createdEvent.Sessions![0].SessionId;

        // Non-OrgAdmin student (Sarah) fails to create event (403 Forbidden)
        SetAuthHeader("std-sarah", "sarah@teamup.edu", "Student");
        var unauthCreate = await _client.PostAsJsonAsync("/api/v1/events", eventPayload);
        Assert.Equal(HttpStatusCode.Forbidden, unauthCreate.StatusCode);

        // B. Sarah registers for the event (#78) -> Confirmed (Seat 1)
        var reg1Res = await _client.PostAsync($"/api/v1/events/{eventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, reg1Res.StatusCode);
        var reg1 = await reg1Res.Content.ReadFromJsonAsync<EventRegistrationResponseDto>();
        Assert.Equal("Confirmed", reg1!.Status);

        // Duplicate registration attempt by Sarah -> 409 Conflict
        var dupReg = await _client.PostAsync($"/api/v1/events/{eventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Conflict, dupReg.StatusCode);

        // C. Ali registers for the event (#78) -> Event full, so Ali gets Waitlisted
        SetAuthHeader("std-ali", "ali@teamup.edu", "Student");
        var reg2Res = await _client.PostAsync($"/api/v1/events/{eventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, reg2Res.StatusCode);
        var reg2 = await reg2Res.Content.ReadFromJsonAsync<EventRegistrationResponseDto>();
        Assert.Equal("Waitlisted", reg2!.Status);

        // D. OrgAdmin lists registrations (#77)
        SetAuthHeader("std-ibrahim", "ibrahim@teamup.edu", "Student");
        var listRegRes = await _client.GetFromJsonAsync<EventRegistrationListResponseDto>($"/api/v1/events/{eventId}/registrations");
        Assert.Equal(2, listRegRes!.TotalCount);

        // E. Record Sarah's attendance (#81) -> Awards +100 points
        var checkinRes = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/attendance",
            new AttendanceVerifyRequestDto("std-sarah", "QRCode")
        );
        Assert.Equal(HttpStatusCode.Created, checkinRes.StatusCode);

        // Check Sarah's score (#83) -> should now have 100 points and Level 2
        SetAuthHeader("std-sarah", "sarah@teamup.edu", "Student");
        var scoreRes = await _client.GetFromJsonAsync<StudentScoreResponseDto>("/api/v1/students/std-sarah/score");
        Assert.Equal(100, scoreRes!.TotalPoints);
        Assert.Equal(2, scoreRes.Level);

        // Check Sarah's badges (#90) -> "C# Pioneer" badge automatically unlocked!
        var badgesRes = await _client.GetFromJsonAsync<List<StudentBadgeResponseDto>>("/api/v1/students/std-sarah/badges");
        Assert.Contains(badgesRes!, b => b.BadgeName == "C# Pioneer");

        // Check Sarah's notifications (#92) -> contains AttendanceVerified and BadgeUnlocked
        var notifsRes = await _client.GetFromJsonAsync<NotificationListResponseDto>("/api/v1/notifications");
        Assert.True(notifsRes!.TotalCount >= 2);
        Assert.Contains(notifsRes.Items, n => n.Type == "AttendanceVerified");
        Assert.Contains(notifsRes.Items, n => n.Type == "BadgeUnlocked");

        // F. Sarah CANNOT cancel registration after attendance is recorded (409 Conflict)
        var cancelBlocked = await _client.DeleteAsync($"/api/v1/events/{eventId}/registrations");
        Assert.Equal(HttpStatusCode.Conflict, cancelBlocked.StatusCode);

        // G. OrgAdmin revokes Sarah's attendance (#82) -> Compensating -100 points record
        SetAuthHeader("std-ibrahim", "ibrahim@teamup.edu", "Student");
        var revokeRes = await _client.DeleteAsync($"/api/v1/sessions/{sessionId}/attendance/std-sarah");
        Assert.Equal(HttpStatusCode.OK, revokeRes.StatusCode);

        // Check updated score -> points reversed to 0, Level back to 1
        SetAuthHeader("std-sarah", "sarah@teamup.edu", "Student");
        var scoreAfterRev = await _client.GetFromJsonAsync<StudentScoreResponseDto>("/api/v1/students/std-sarah/score");
        Assert.Equal(0, scoreAfterRev!.TotalPoints);
        Assert.Equal(1, scoreAfterRev.Level);

        // Now that attendance was revoked, Sarah can cancel her registration (#79)
        var cancelAllowed = await _client.DeleteAsync($"/api/v1/events/{eventId}/registrations");
        Assert.Equal(HttpStatusCode.OK, cancelAllowed.StatusCode);

        // Waitlist FIFO promotion: Ali should now be promoted to Confirmed!
        SetAuthHeader("std-ibrahim", "ibrahim@teamup.edu", "Student");
        var listAfterCancel = await _client.GetFromJsonAsync<EventRegistrationListResponseDto>($"/api/v1/events/{eventId}/registrations");
        var aliReg = listAfterCancel!.Items.First(r => r.StudentId == "std-ali");
        Assert.Equal("Confirmed", aliReg.Status);
    }

    [Fact]
    public async Task Dev_TestTokens_Returns_Ok_With_All_Required_Roles()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/dev/test-tokens");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("student1@teamup.edu", content);
        Assert.Contains("orgadmin1@teamup.edu", content);
        Assert.Contains("platformadmin1@teamup.edu", content);
    }

    [Fact]
    public async Task Protected_Endpoints_Return_401_When_Unauthenticated()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var eventsRes = await _client.GetAsync("/api/v1/events");
        Assert.Equal(HttpStatusCode.Unauthorized, eventsRes.StatusCode);

        var notificationsRes = await _client.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.Unauthorized, notificationsRes.StatusCode);

        var badgesRes = await _client.GetAsync("/api/v1/badges");
        Assert.Equal(HttpStatusCode.Unauthorized, badgesRes.StatusCode);
    }

    [Fact]
    public async Task Student_Cannot_Access_PlatformAdmin_Endpoints()
    {
        SetAuthHeader("1", "student1@teamup.edu", "Student");

        // Attempt to create badge (only PlatformAdmin)
        var createBadge = await _client.PostAsJsonAsync("/api/v1/badges", new BadgeCreateDto(
            "Hacker Badge",
            "Illegal creation attempt",
            "https://hack.com/icon.png",
            "TotalPoints",
            500
        ));
        Assert.Equal(HttpStatusCode.Forbidden, createBadge.StatusCode);

        // Attempt manual score adjustment (only PlatformAdmin)
        var adjustScore = await _client.PostAsJsonAsync("/api/v1/score-transactions", new ScoreTransactionCreateDto
        {
            StudentId = "1",
            Points = 1000,
            Reason = "Free points"
        });
        Assert.Equal(HttpStatusCode.Forbidden, adjustScore.StatusCode);
    }

    [Fact]
    public async Task Student_Cannot_View_Other_Students_Private_Data()
    {
        SetAuthHeader("1", "student1@teamup.edu", "Student");

        // Student 1 tries to view std-sarah's score
        var scoreRes = await _client.GetAsync("/api/v1/students/std-sarah/score");
        Assert.Equal(HttpStatusCode.Forbidden, scoreRes.StatusCode);

        // Student 1 tries to view std-sarah's badges
        var badgesRes = await _client.GetAsync("/api/v1/students/std-sarah/badges");
        Assert.Equal(HttpStatusCode.Forbidden, badgesRes.StatusCode);
    }
}

