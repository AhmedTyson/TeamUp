using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class EventsAttendanceGamificationTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _anonymous = factory.CreateClient();

    private HttpClient CreateClientWithRole(int studentId, string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"user_{studentId}@teamup.test", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> GetDataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task GamificationAndBadges_FullFlow_WorksAsExpected()
    {
        var admin = CreateClientWithRole(902, "PlatformAdmin");

        // 1. Register student
        var email = $"gamer_{Guid.NewGuid():N}@teamup.test";
        var reg = new RegisterRequest("Gamified", "Student", email, "P@ssword123!", 2, factory.SeededFacultyId);
        var regRes = await _anonymous.PostAsJsonAsync("/api/v1/auth/register", reg);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var studentId = auth.Student.StudentId;

        // 2. Initial score is 0, Level 1
        var scoreRes = await student.GetAsync($"/api/v1/students/{studentId}/score");
        Assert.Equal(HttpStatusCode.OK, scoreRes.StatusCode);
        var score = await GetDataAsync<StudentScoreDto>(scoreRes);
        Assert.Equal(0, score.TotalPoints);
        Assert.Equal(1, score.Level);

        // 3. Admin awards manual bonus points (+250 points)
        var addPtsRes = await admin.PostAsJsonAsync("/api/v1/score-transactions",
            new CreateScoreTransactionRequest(studentId, 250, "Hackathon Winner Prize", ScoreSourceType.Manual));
        Assert.Equal(HttpStatusCode.Created, addPtsRes.StatusCode);

        // 4. Score updates to Level 2 (250 pts >= 100 pts threshold, next is 400 pts)
        var newScoreRes = await student.GetAsync($"/api/v1/students/{studentId}/score");
        var newScore = await GetDataAsync<StudentScoreDto>(newScoreRes);
        Assert.Equal(250, newScore.TotalPoints);
        Assert.Equal(2, newScore.Level);
        Assert.Equal(100, newScore.CurrentLevelStart);
        Assert.Equal(400, newScore.NextLevelThreshold);
        Assert.Equal(150, newScore.PointsToNext);

        // 5. Admin creates badge in catalog
        var badgeName = $"Master Hacker {Guid.NewGuid():N}";
        var createBadgeRes = await admin.PostAsJsonAsync("/api/v1/badges",
            new CreateBadgeRequest(badgeName, "Win hackathon", "https://cdn.teamup.edu/badge.png", BadgeCriteriaType.PointsThreshold, 200));
        Assert.Equal(HttpStatusCode.Created, createBadgeRes.StatusCode);
        var badge = await GetDataAsync<BadgeDto>(createBadgeRes);

        // 6. Admin manually awards badge to student
        var awardRes = await admin.PostAsJsonAsync($"/api/v1/students/{studentId}/badges/{badge.BadgeId}", new { });
        Assert.Equal(HttpStatusCode.Created, awardRes.StatusCode);

        // 7. Student retrieves badges
        var badgesRes = await student.GetAsync($"/api/v1/students/{studentId}/badges");
        Assert.Equal(HttpStatusCode.OK, badgesRes.StatusCode);
        var badges = await GetDataAsync<List<StudentBadgeDto>>(badgesRes);
        Assert.Contains(badges, b => b.BadgeId == badge.BadgeId);

        // 8. Student checks notification inbox
        var notifsRes = await student.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.OK, notifsRes.StatusCode);
        var notifs = await GetDataAsync<List<NotificationDto>>(notifsRes);
        Assert.NotEmpty(notifs);

        // 9. Student marks notification as read
        var firstNotif = notifs[0];
        var markReadRes = await student.PatchAsync($"/api/v1/notifications/{firstNotif.NotificationId}", null);
        Assert.Equal(HttpStatusCode.NoContent, markReadRes.StatusCode);

        // 10. Student deletes notification
        var deleteNotifRes = await student.DeleteAsync($"/api/v1/notifications/{firstNotif.NotificationId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteNotifRes.StatusCode);
    }
}
