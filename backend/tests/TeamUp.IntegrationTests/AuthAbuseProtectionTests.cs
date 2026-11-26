// Date: 2026-11-24
// Purpose: Regression tests for per-account login throttling (TU-SEC-009) and refresh-token reuse detection (TU-SEC-011).

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class AuthAbuseProtectionTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password), ApiHelper.Json);

    private Task<HttpResponseMessage> RefreshAsync(string token) =>
        factory.CreateClient().PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(token), ApiHelper.Json);

    private static string Message(string json) =>
        System.Text.Json.JsonElement.Parse(json).GetProperty("message").GetString()!;

    [Fact]
    public async Task RepeatedWrongPasswords_TriggerPerAccountDelay_EvenForCorrectPassword_WithIdenticalResponse()
    {
        var (_, _, email, _) = await _api.RegisterAsync("lockout");

        HttpResponseMessage? last = null;
        for (var i = 0; i < 6; i++)
            last = await LoginAsync(email, "wrong-password-xyz");
        Assert.Equal(HttpStatusCode.Unauthorized, last!.StatusCode);
        var wrongBody = Message(await last.Content.ReadAsStringAsync());

        // Account is now delayed: the correct password is refused with the same generic response.
        var correct = await LoginAsync(email, "P@ssword123!");
        Assert.Equal(HttpStatusCode.Unauthorized, correct.StatusCode);
        Assert.Equal(wrongBody, Message(await correct.Content.ReadAsStringAsync()));

        // Delay is temporary, not permanent: once it expires the owner can sign in and the counter resets.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var normalizedEmail = email.ToLowerInvariant();
            var s = await db.Students.SingleAsync(x => x.Email == normalizedEmail);
            Assert.True(s.LockoutEndUtc > DateTime.UtcNow);
            s.LockoutEndUtc = DateTime.UtcNow.AddSeconds(-1);
            await db.SaveChangesAsync();
        }
        var after = await LoginAsync(email, "P@ssword123!");
        Assert.Equal(HttpStatusCode.OK, after.StatusCode);
    }

    [Fact]
    public async Task FewWrongPasswords_DoNotLockLegitimateUser()
    {
        var (_, _, email, _) = await _api.RegisterAsync("fewfail");
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync(email, "nope-nope-nope")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(email, "P@ssword123!")).StatusCode);
    }

    [Fact]
    public async Task RefreshReplayOfOldToken_AfterGraceWindow_RevokesSession()
    {
        var (_, studentId, _, auth) = await _api.RegisterAsync("reuse");
        var r1 = auth.RefreshToken;

        var rotated = await RefreshAsync(r1);
        Assert.Equal(HttpStatusCode.OK, rotated.StatusCode);
        var r2 = (await ApiHelper.DataAsync<AuthResponse>(rotated)).RefreshToken;

        // Immediate replay is rejected but treated as a client race: the new token keeps working.
        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(r1)).StatusCode);
        var r3Res = await RefreshAsync(r2);
        Assert.Equal(HttpStatusCode.OK, r3Res.StatusCode);
        var r3 = (await ApiHelper.DataAsync<AuthResponse>(r3Res)).RefreshToken;

        // Age the rotation beyond the grace window, then replay the stale token (theft signal).
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var s = await db.Students.SingleAsync(x => x.StudentId == studentId);
            s.PreviousRefreshTokenRotatedAt = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(r2)).StatusCode);

        // The legitimate current token is revoked too: the whole session is dead.
        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshAsync(r3)).StatusCode);
    }

    [Fact]
    public async Task NormalRotation_KeepsWorking()
    {
        var (_, _, _, auth) = await _api.RegisterAsync("rot");
        var a = await RefreshAsync(auth.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, a.StatusCode);
        var t2 = (await ApiHelper.DataAsync<AuthResponse>(a)).RefreshToken;
        var b = await RefreshAsync(t2);
        Assert.Equal(HttpStatusCode.OK, b.StatusCode);
    }
}
