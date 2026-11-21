// Date: 2026-11-21
// Purpose: End-to-end integration tests verifying all STRIDE mitigations (BOLA, SecurityStamp revocation, audit logs, rotating QR check-in, directory privacy, rate limiting, SSRF, escalation guard) and automated gamification/notification workflows.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Students;
using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Services;

namespace TeamUp.IntegrationTests;

public class StrideSecurityAndBackendCompletionTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _anonymous = factory.CreateClient();

    private HttpClient CreateRoleClient(int studentId, string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"stride_{studentId}@teamup.test", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<(HttpClient Client, AuthResponse Auth)> RegisterStudentAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@teamup.test";
        var res = await _anonymous.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(prefix, "Tester", email, "P@ssword123!", 3, factory.SeededFacultyId));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var auth = await DataAsync<AuthResponse>(res);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    private static async Task<T> DataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOpts);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task BolaProtection_OrgAdminOfClubA_CannotModifyClubBOrItsEventsAndAttendance()
    {
        using var adminA = CreateRoleClient(5101, "OrgAdmin");
        using var adminB = CreateRoleClient(5102, "OrgAdmin");
        var (memberClient, memberAuth) = await RegisterStudentAsync("bola_member");

        var orgARes = await adminA.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"ClubA_{Guid.NewGuid():N}", "Club A", "Tech", null));
        Assert.Equal(HttpStatusCode.Created, orgARes.StatusCode);
        var orgA = await DataAsync<OrganizationDetailDto>(orgARes);

        var orgBRes = await adminB.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"ClubB_{Guid.NewGuid():N}", "Club B", "Tech", null));
        Assert.Equal(HttpStatusCode.Created, orgBRes.StatusCode);
        var orgB = await DataAsync<OrganizationDetailDto>(orgBRes);

        // Creator of Org A is automatically a scoped Admin member
        Assert.Contains(orgA.Members, m => m.StudentId == 5101 && m.Role == OrganizationRoles.Admin);

        // OrgAdmin B creates event & session under Org B
        var start = DateTime.UtcNow.AddDays(3);
        var evtBRes = await adminB.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Club B Summit", "Private summit", EventType.Hackathon, "Hall B", start, start.AddDays(1), 50, orgB.OrganizationId));
        Assert.Equal(HttpStatusCode.Created, evtBRes.StatusCode);
        var evtB = await DataAsync<EventDto>(evtBRes);

        var sessBRes = await adminB.PostAsJsonAsync($"/api/v1/events/{evtB.EventId}/sessions",
            new CreateEventSessionRequest(1, "Keynote", start.AddHours(1), start.AddHours(2), "Room 2"));
        Assert.Equal(HttpStatusCode.Created, sessBRes.StatusCode);
        var sessB = await DataAsync<EventSessionDto>(sessBRes);

        // Member registers for Org B event
        var regRes = await memberClient.PostAsync($"/api/v1/events/{evtB.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);

        // OrgAdmin A MUST get 403 Forbidden on every Org B / Event B / Session B management action (STRIDE THR-T-01, THR-T-02, THR-E-01)
        var patchOrgB = await adminA.PatchAsJsonAsync($"/api/v1/organizations/{orgB.OrganizationId}",
            new UpdateOrganizationRequest("Hijacked", "Hijacked", "Tech", null));
        Assert.Equal(HttpStatusCode.Forbidden, patchOrgB.StatusCode);

        var appsOrgB = await adminA.GetAsync($"/api/v1/organizations/{orgB.OrganizationId}/applications");
        Assert.Equal(HttpStatusCode.Forbidden, appsOrgB.StatusCode);

        var createEvtInB = await adminA.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Spoofed", "Desc", EventType.Workshop, "Loc", start, start.AddDays(1), 10, orgB.OrganizationId));
        Assert.Equal(HttpStatusCode.Forbidden, createEvtInB.StatusCode);

        var patchEvtB = await adminA.PatchAsJsonAsync($"/api/v1/events/{evtB.EventId}",
            new UpdateEventRequest("Hijacked Event", "Desc", EventType.Workshop, "Loc", start, start.AddDays(1), 10));
        Assert.Equal(HttpStatusCode.Forbidden, patchEvtB.StatusCode);

        var updateSessB = await adminA.PutAsJsonAsync($"/api/v1/sessions/{sessB.SessionId}",
            new UpdateEventSessionRequest(1, "Hijacked Session", start.AddHours(1), start.AddHours(2), "X"));
        Assert.Equal(HttpStatusCode.Forbidden, updateSessB.StatusCode);

        var regsB = await adminA.GetAsync($"/api/v1/events/{evtB.EventId}/registrations");
        Assert.Equal(HttpStatusCode.Forbidden, regsB.StatusCode);

        var checkInB = await adminA.PostAsJsonAsync($"/api/v1/sessions/{sessB.SessionId}/attendance",
            new CreateAttendanceRequest(memberAuth.Student.StudentId, VerificationMethod.ManualByOrganizer));
        Assert.Equal(HttpStatusCode.Forbidden, checkInB.StatusCode);

        var reportB = await adminA.GetAsync($"/api/v1/sessions/{sessB.SessionId}/attendance/report");
        Assert.Equal(HttpStatusCode.Forbidden, reportB.StatusCode);

        // Scoped OrgAdmin via OrganizationMember.Role = "Admin" works for a regular Student WITHOUT global OrgAdmin role!
        var (scopedStudent, scopedAuth) = await RegisterStudentAsync("scoped_exec");
        var applyA = await scopedStudent.PostAsJsonAsync($"/api/v1/organizations/{orgA.OrganizationId}/applications",
            new CreateMembershipApplicationRequest("Let me lead"));
        Assert.Equal(HttpStatusCode.Created, applyA.StatusCode);
        var appA = await DataAsync<MembershipApplicationDto>(applyA);

        var acceptA = await adminA.PatchAsJsonAsync($"/api/v1/organizations/{orgA.OrganizationId}/applications/{appA.ApplicationId}",
            new ReviewMembershipApplicationRequest(ApplicationStatus.Accepted));
        Assert.Equal(HttpStatusCode.OK, acceptA.StatusCode);

        var promoteExec = await adminA.PatchAsJsonAsync($"/api/v1/organizations/{orgA.OrganizationId}/members/{scopedAuth.Student.StudentId}",
            new UpdateOrganizationMemberRequest(OrganizationRoles.Admin));
        Assert.Equal(HttpStatusCode.OK, promoteExec.StatusCode);

        // Regular Student who is OrganizationMember.Admin of Club A can manage Club A, but still gets 403 on Club B!
        var scopedEventRes = await scopedStudent.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Scoped Event", "Hosted by scoped exec", EventType.Session, "Room 1", start, start.AddDays(1), 20, orgA.OrganizationId));
        Assert.Equal(HttpStatusCode.Created, scopedEventRes.StatusCode);

        var scopedCrossOrg = await scopedStudent.PatchAsJsonAsync($"/api/v1/organizations/{orgB.OrganizationId}",
            new UpdateOrganizationRequest("Nope", "Nope", "Tech", null));
        Assert.Equal(HttpStatusCode.Forbidden, scopedCrossOrg.StatusCode);

        // Acceptance also delivered an in-app notification to the applicant
        var notifsRes = await scopedStudent.GetAsync("/api/v1/notifications");
        var notifs = await DataAsync<List<NotificationDto>>(notifsRes);
        Assert.Contains(notifs, n => n.Type == NotificationType.ApplicationStatus);
    }

    [Fact]
    public async Task RotatingQrCheckIn_RecordsOrigin_AwardsXp_AndRendersHtmlReport()
    {
        using var organizer = CreateRoleClient(5201, "OrgAdmin");
        var (student1, auth1) = await RegisterStudentAsync("qr_student1");
        var (student2, auth2) = await RegisterStudentAsync("qr_student2");

        var orgRes = await organizer.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest($"QROrg_{Guid.NewGuid():N}", "QR Club", "Technology", null));
        var org = await DataAsync<OrganizationDetailDto>(orgRes);

        var start = DateTime.UtcNow.AddDays(2);
        var evtRes = await organizer.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("QR Workshop", "Hands-on", EventType.Workshop, "Lab 1", start, start.AddDays(1), 10, org.OrganizationId));
        var evt = await DataAsync<EventDto>(evtRes);

        var sessRes = await organizer.PostAsJsonAsync($"/api/v1/events/{evt.EventId}/sessions",
            new CreateEventSessionRequest(1, "Session 1", start.AddHours(1), start.AddHours(3), "Lab 101"));
        var sess = await DataAsync<EventSessionDto>(sessRes);

        Assert.Equal(HttpStatusCode.Created, (await student1.PostAsync($"/api/v1/events/{evt.EventId}/registrations", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await student2.PostAsync($"/api/v1/events/{evt.EventId}/registrations", null)).StatusCode);

        // Tampered code fails
        var badSelf = await student1.PostAsJsonAsync($"/api/v1/sessions/{sess.SessionId}/attendance/self-check-in",
            new SelfCheckInRequest("0.deadbeefdeadbeef"));
        Assert.Equal(HttpStatusCode.BadRequest, badSelf.StatusCode);

        // Organizer generates valid 60s code
        var codeRes = await organizer.PostAsync($"/api/v1/sessions/{sess.SessionId}/attendance/check-in-code", null);
        Assert.Equal(HttpStatusCode.OK, codeRes.StatusCode);
        var codeDto = await DataAsync<SessionCheckInCodeDto>(codeRes);
        Assert.False(string.IsNullOrWhiteSpace(codeDto.Code));

        // Student 1 self checks in with valid rotating code
        var goodSelf = await student1.PostAsJsonAsync($"/api/v1/sessions/{sess.SessionId}/attendance/self-check-in",
            new SelfCheckInRequest(codeDto.Code));
        Assert.Equal(HttpStatusCode.Created, goodSelf.StatusCode);
        var att1 = await DataAsync<SessionAttendanceDto>(goodSelf);
        Assert.Equal(auth1.Student.StudentId, att1.RecordedByStudentId);

        // Organizer checks in Student 2 -> RecordedByStudentId captures organizer ID (STRIDE THR-R-02)
        var orgCheckIn = await organizer.PostAsJsonAsync($"/api/v1/sessions/{sess.SessionId}/attendance",
            new CreateAttendanceRequest(auth2.Student.StudentId, VerificationMethod.ManualByOrganizer));
        Assert.Equal(HttpStatusCode.Created, orgCheckIn.StatusCode);
        var att2 = await DataAsync<SessionAttendanceDto>(orgCheckIn);
        Assert.Equal(5201, att2.RecordedByStudentId);

        // Automatic XP (+25) awarded via SessionAttendedDomainEvent
        var score1 = await DataAsync<StudentScoreDto>(await student1.GetAsync($"/api/v1/students/{auth1.Student.StudentId}/score"));
        Assert.Equal(25, score1.TotalPoints);

        // Printable HTML report rendered from Templates/Reports/SessionAttendanceReport.html
        var reportRes = await organizer.GetAsync($"/api/v1/sessions/{sess.SessionId}/attendance/report");
        Assert.Equal(HttpStatusCode.OK, reportRes.StatusCode);
        Assert.Equal("text/html", reportRes.Content.Headers.ContentType?.MediaType);
        var html = await reportRes.Content.ReadAsStringAsync();
        Assert.Contains("QR Workshop", html);
        Assert.Contains("5201", html);

        // Event discovery filters by type & date range
        var filteredEvents = await DataAsync<PagedResult<EventDto>>(
            await student1.GetAsync($"/api/v1/events?organizationId={org.OrganizationId}&type=Workshop&from={Uri.EscapeDataString(start.AddHours(-1).ToString("O"))}&to={Uri.EscapeDataString(start.AddDays(2).ToString("O"))}"));
        Assert.Contains(filteredEvents.Items, e => e.EventId == evt.EventId);
        var wrongTypeEvents = await DataAsync<PagedResult<EventDto>>(
            await student1.GetAsync($"/api/v1/events?organizationId={org.OrganizationId}&type=Competition"));
        Assert.DoesNotContain(wrongTypeEvents.Items, e => e.EventId == evt.EventId);
    }

    [Fact]
    public async Task SecurityStamp_ImmediateRevocationOnRoleChangeLogoutAndPasswordReset()
    {
        using var admin = CreateRoleClient(5301, "PlatformAdmin");
        var (studentClient, auth) = await RegisterStudentAsync("stamp_user");

        // 1. Works initially
        Assert.Equal(HttpStatusCode.OK, (await studentClient.GetAsync("/api/v1/students/me")).StatusCode);

        // 2. Role change immediately invalidates existing JWT (STRIDE THR-E-02)
        var roleRes = await admin.PatchAsJsonAsync($"/api/v1/admin/students/{auth.Student.StudentId}/role",
            new UpdateUserRoleRequest(UserRole.OrgAdmin));
        Assert.Equal(HttpStatusCode.OK, roleRes.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await studentClient.GetAsync("/api/v1/students/me")).StatusCode);

        // 3. Re-login gets fresh token; logout immediately invalidates access token too
        var loginRes = await _anonymous.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(auth.Student.Email, "P@ssword123!"));
        var loginAuth = await DataAsync<AuthResponse>(loginRes);

        using var loggedInClient = factory.CreateClient();
        loggedInClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginAuth.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await loggedInClient.GetAsync("/api/v1/students/me")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await loggedInClient.PostAsync("/api/v1/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await loggedInClient.GetAsync("/api/v1/students/me")).StatusCode);

        // 4. Re-login -> reset password -> old access token immediately revoked
        var login2 = await DataAsync<AuthResponse>(await _anonymous.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(auth.Student.Email, "P@ssword123!")));
        using var preResetClient = factory.CreateClient();
        preResetClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login2.AccessToken);

        await _anonymous.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(auth.Student.Email));
        var resetToken = factory.Services.GetRequiredService<InMemoryEmailSender>().GetLastResetToken(auth.Student.Email);
        Assert.NotNull(resetToken);

        var resetRes = await _anonymous.PostAsJsonAsync("/api/v1/auth/reset-password",
            new ResetPasswordRequest(auth.Student.Email, resetToken!, "NewStrongPass2026!"));
        Assert.Equal(HttpStatusCode.OK, resetRes.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await preResetClient.GetAsync("/api/v1/students/me")).StatusCode);
    }

    [Fact]
    public async Task AdminGovernance_BlocksPlatformAdminEscalation_AndRecordsImmutableAuditLogs()
    {
        using var admin = CreateRoleClient(5401, "PlatformAdmin");
        var (_, targetAuth) = await RegisterStudentAsync("escalate_target");

        // STRIDE THR-E-03: runtime promotion to PlatformAdmin is blocked by default
        var escalateRes = await admin.PatchAsJsonAsync($"/api/v1/admin/students/{targetAuth.Student.StudentId}/role",
            new UpdateUserRoleRequest(UserRole.PlatformAdmin));
        Assert.Equal(HttpStatusCode.Forbidden, escalateRes.StatusCode);

        // Permitted promotion to OrgAdmin writes immutable AuditLog (STRIDE THR-R-01)
        var promoteRes = await admin.PatchAsJsonAsync($"/api/v1/admin/students/{targetAuth.Student.StudentId}/role",
            new UpdateUserRoleRequest(UserRole.OrgAdmin));
        Assert.Equal(HttpStatusCode.OK, promoteRes.StatusCode);

        var logsRes = await admin.GetAsync("/api/v1/admin/audit-logs?action=RoleChanged");
        Assert.Equal(HttpStatusCode.OK, logsRes.StatusCode);
        var logs = await DataAsync<PagedResult<AuditLogDto>>(logsRes);
        Assert.Contains(logs.Items, l =>
            l.EntityType == "Student"
            && l.EntityId == targetAuth.Student.StudentId.ToString()
            && l.ActorStudentId == 5401);
    }

    [Fact]
    public async Task StudentDirectory_EnforcesPageSizeCap20AndEmailPrivacyPreferences()
    {
        var (studentA, authA) = await RegisterStudentAsync("privacyA");
        var (studentB, _) = await RegisterStudentAsync("privacyB");

        // Student B queries directory requesting pageSize=100 -> capped at 20, Student A email hidden by default (STRIDE THR-I-01)
        var res = await studentB.GetAsync($"/api/v1/students?search=privacyA&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var page = await DataAsync<PagedResult<StudentCardDto>>(res);
        Assert.Equal(20, page.PageSize);
        var cardA = Assert.Single(page.Items, s => s.StudentId == authA.Student.StudentId);
        Assert.Null(cardA.Email);

        // Searching by Student A's private email yields zero matches for Student B
        var emailProbe = await DataAsync<PagedResult<StudentCardDto>>(
            await studentB.GetAsync($"/api/v1/students?search={Uri.EscapeDataString(authA.Student.Email)}"));
        Assert.DoesNotContain(emailProbe.Items, s => s.StudentId == authA.Student.StudentId);

        // Student A opts into email visibility
        var optIn = await studentA.PatchAsJsonAsync("/api/v1/students/me", new UpdateStudentRequest(null, null, null, ShowEmailInDirectory: true));
        Assert.Equal(HttpStatusCode.OK, optIn.StatusCode);

        var afterOptIn = await DataAsync<PagedResult<StudentCardDto>>(
            await studentB.GetAsync($"/api/v1/students?search=privacyA"));
        var visibleCardA = Assert.Single(afterOptIn.Items, s => s.StudentId == authA.Student.StudentId);
        Assert.Equal(authA.Student.Email, visibleCardA.Email);
    }

    [Fact]
    public async Task GamificationAutomation_TaskDoneAwardsXpAndTaskCountBadge_EnforcesLedgerBola_AndServesLeaderboard()
    {
        using var admin = CreateRoleClient(5501, "PlatformAdmin");
        var (creator, creatorAuth) = await RegisterStudentAsync("proj_lead");
        var (member, memberAuth) = await RegisterStudentAsync("proj_dev");

        // Create TaskCount badge (CriteriaValue = 1)
        var badgeName = $"FirstTaskDone_{Guid.NewGuid():N}";
        var badgeRes = await admin.PostAsJsonAsync("/api/v1/badges",
            new CreateBadgeRequest(badgeName, "Finish 1 task", "/badges/task-done.png", BadgeCriteriaType.TaskCount, 1));
        Assert.Equal(HttpStatusCode.Created, badgeRes.StatusCode);

        // Create project, apply, accept, create & assign task, mark Done, then complete project
        var project = await DataAsync<ProjectDetailDto>(await creator.PostAsJsonAsync("/api/v1/projects",
            new CreateProjectRequest("AutoXP Project", "Testing automated domain events", 3)));
        var app = await DataAsync<ProjectApplicationDto>(await member.PostAsJsonAsync($"/api/v1/projects/{project.ProjectId}/applications",
            new CreateProjectApplicationRequest("Count me in")));
        Assert.Equal(HttpStatusCode.OK, (await creator.PatchAsJsonAsync($"/api/v1/projects/{project.ProjectId}/applications/{app.ApplicationId}",
            new ReviewProjectApplicationRequest(ApplicationStatus.Accepted))).StatusCode);

        var task = await DataAsync<ProjectTaskDto>(await creator.PostAsJsonAsync($"/api/v1/projects/{project.ProjectId}/tasks",
            new CreateProjectTaskRequest("Ship Security Fix", "STRIDE hardening", DateTime.UtcNow.AddDays(1))));
        Assert.Equal(HttpStatusCode.NoContent, (await creator.PutAsJsonAsync($"/api/v1/tasks/{task.TaskId}/assignee",
            new AssignTaskRequest(memberAuth.Student.StudentId))).StatusCode);

        // Mark task Done -> +50 XP + TaskCount badge automatically unlocked!
        Assert.Equal(HttpStatusCode.OK, (await member.PatchAsJsonAsync($"/api/v1/tasks/{task.TaskId}",
            new UpdateProjectTaskRequest(null, null, ProjectTaskStatus.Done, null, null))).StatusCode);

        var memberScore = await DataAsync<StudentScoreDto>(await member.GetAsync($"/api/v1/students/{memberAuth.Student.StudentId}/score"));
        Assert.Equal(50, memberScore.TotalPoints);

        var memberBadges = await DataAsync<List<StudentBadgeDto>>(await member.GetAsync($"/api/v1/students/{memberAuth.Student.StudentId}/badges"));
        Assert.Contains(memberBadges, b => b.BadgeName == badgeName);

        // Notifications received for application acceptance, task assignment, and badge unlock
        var notifs = await DataAsync<List<NotificationDto>>(await member.GetAsync("/api/v1/notifications"));
        Assert.Contains(notifs, n => n.Type == NotificationType.ApplicationStatus);
        Assert.Contains(notifs, n => n.Type == NotificationType.TaskAssigned);
        Assert.Contains(notifs, n => n.Type == NotificationType.BadgeEarned);

        // Mark project Completed -> +100 XP to both creator and member
        Assert.Equal(HttpStatusCode.OK, (await creator.PatchAsJsonAsync($"/api/v1/projects/{project.ProjectId}",
            new UpdateProjectRequest(null, null, ProjectStatus.Completed, null))).StatusCode);
        var finalMemberScore = await DataAsync<StudentScoreDto>(await member.GetAsync($"/api/v1/students/{memberAuth.Student.StudentId}/score"));
        Assert.Equal(150, finalMemberScore.TotalPoints);

        // Soft-delete project and restore via universal admin restore -> logged in AuditLogs
        Assert.Equal(HttpStatusCode.NoContent, (await creator.DeleteAsync($"/api/v1/projects/{project.ProjectId}")).StatusCode);
        var restoreProj = await admin.PostAsync($"/api/v1/admin/restore/project/{project.ProjectId}", null);
        Assert.Equal(HttpStatusCode.OK, restoreProj.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await creator.GetAsync($"/api/v1/projects/{project.ProjectId}")).StatusCode);

        var restoreLogs = await DataAsync<PagedResult<AuditLogDto>>(
            await admin.GetAsync("/api/v1/admin/audit-logs?action=EntityRestored&entityType=Project"));
        Assert.Contains(restoreLogs.Items, l => l.EntityId == project.ProjectId.ToString());

        // Ledger BOLA: creator cannot read member's private score transactions (403 Forbidden), member & admin can
        var otherLedger = await creator.GetAsync($"/api/v1/students/{memberAuth.Student.StudentId}/score-transactions");
        Assert.Equal(HttpStatusCode.Forbidden, otherLedger.StatusCode);
        var ownLedger = await member.GetAsync($"/api/v1/students/{memberAuth.Student.StudentId}/score-transactions");
        Assert.Equal(HttpStatusCode.OK, ownLedger.StatusCode);

        // Leaderboard returns member ranked with >= 150 points
        var leaderboardRes = await member.GetAsync("/api/v1/gamification/leaderboard?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, leaderboardRes.StatusCode);
        var leaderboard = await DataAsync<PagedResult<LeaderboardEntryDto>>(leaderboardRes);
        Assert.Contains(leaderboard.Items, e => e.StudentId == memberAuth.Student.StudentId && e.TotalPoints >= 150);

        // Unused variable guard
        Assert.True(creatorAuth.Student.StudentId > 0);
    }

    [Fact]
    public async Task RateLimiting_GlobalAndSpoofedForwardedForCannotBypass_AndSecurityHeadersPresent()
    {
        await using var tightFactory = new TightRateLimitFactory();
        using var client = tightFactory.CreateClient();

        // Verify security headers & database health check
        var healthRes = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthRes.StatusCode);
        Assert.Equal("nosniff", healthRes.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", healthRes.Headers.GetValues("X-Frame-Options").Single());
        var healthJson = await healthRes.Content.ReadAsStringAsync();
        Assert.Contains("Connected", healthJson);

        // Send 3 anonymous search requests with DIFFERENT spoofed X-Forwarded-For headers -> 4th MUST be rejected with 429 (STRIDE THR-S-02, THR-D-01)
        for (var i = 1; i <= 3; i++)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/universities");
            req.Headers.Add("X-Forwarded-For", $"203.0.113.{i}");
            var okRes = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, okRes.StatusCode);
        }

        using var fourthReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/universities");
        fourthReq.Headers.Add("X-Forwarded-For", "203.0.113.99");
        var throttled = await client.SendAsync(fourthReq);
        Assert.Equal(HttpStatusCode.TooManyRequests, throttled.StatusCode);
        Assert.True(throttled.Headers.Contains("Retry-After"));

        // Health probe remains exempt even after anonymous quota is exhausted
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task SsrfAndXssUrlPayloads_AreRejectedByApiValidation()
    {
        using var admin = CreateRoleClient(5601, "PlatformAdmin");

        var ssrfOrg = await admin.PostAsJsonAsync("/api/v1/organizations",
            new CreateOrganizationRequest("SSRF Org", "Desc", "Tech", "http://169.254.169.254/latest/meta-data/"));
        Assert.Equal(HttpStatusCode.BadRequest, ssrfOrg.StatusCode);

        var xssBadge = await admin.PostAsJsonAsync("/api/v1/badges",
            new CreateBadgeRequest("XSS Badge", "Desc", "javascript:alert(1)", BadgeCriteriaType.PointsThreshold, 10));
        Assert.Equal(HttpStatusCode.BadRequest, xssBadge.StatusCode);
    }

    [Fact]
    public void ProductionStartup_WithMissingOrFallbackJwtKey_FailsFast()
    {
        using var prodFactory = new ProductionMissingKeyFactory();
        var ex = Assert.Throws<InvalidOperationException>(() => prodFactory.CreateClient());
        Assert.Contains("Jwt:Key", ex.Message);
    }

    private sealed class ProductionMissingKeyFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DatabaseProvider"] = "Sqlite",
                    ["Jwt:Key"] = "DevelopmentFallbackSecretKeyForTeamUpAppMustBeAtLeast32BytesLong!",
                    ["Email:Provider"] = "Smtp"
                });
            });
        }
    }

    private sealed class TightRateLimitFactory : TeamUpApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:AnonymousPermitLimit"] = "3",
                    ["RateLimiting:AuthPermitLimit"] = "3",
                    ["RateLimiting:AuthenticatedPermitLimit"] = "5",
                    ["RateLimiting:DirectoryPermitLimit"] = "2",
                    ["RateLimiting:WindowSeconds"] = "60"
                });
            });
        }
    }
}
