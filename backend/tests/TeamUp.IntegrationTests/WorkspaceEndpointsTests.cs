// Date: 2026-11-27
// Purpose: Integration tests for all 9 composite single-request workspace endpoints (including BOLA and privacy enforcement).

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Application.Auth;
using TeamUp.Application.Events;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Workspaces;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class WorkspaceEndpointsTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private async Task<(HttpClient Client, int StudentId)> RegisterRoleAsync(string prefix, string role)
    {
        var (_, studentId, email, _) = await _api.RegisterAsync(prefix);
        var token = factory.CreateToken(studentId, email, role);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, studentId);
    }

    private async Task<(OrganizationDetailDto Org, EventDto Event, EventSessionDto Session)> CreateOrgEventAndSessionAsync(
        HttpClient orgAdminClient, int capacity = 25)
    {
        var orgRes = await orgAdminClient.PostAsJsonAsync("/api/v1/organizations", new CreateOrganizationRequest(
            $"WorkspaceOrg_{Guid.NewGuid():N}", "Org desc", "Engineering", null,
            OrganizationScope.UniversityBranch, factory.SeededUniversityId, "Cairo", IsRecruiting: true), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, orgRes.StatusCode);
        var org = await ApiHelper.DataAsync<OrganizationDetailDto>(orgRes);

        var start = DateTime.UtcNow.AddDays(2);
        var evRes = await orgAdminClient.PostAsJsonAsync("/api/v1/events", new CreateEventRequest(
            $"WorkspaceEvent_{Guid.NewGuid():N}", "Event desc", EventType.Workshop, "Hall A",
            start, start.AddHours(4), capacity, org.OrganizationId), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, evRes.StatusCode);
        var ev = await ApiHelper.DataAsync<EventDto>(evRes);

        var sesRes = await orgAdminClient.PostAsJsonAsync($"/api/v1/events/{ev.EventId}/sessions",
            new CreateEventSessionRequest(1, "Opening Session", start, start.AddHours(2), "Room 101"), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, sesRes.StatusCode);
        var ses = await ApiHelper.DataAsync<EventSessionDto>(sesRes);

        return (org, ev, ses);
    }

    [Fact]
    public async Task StudentDashboard_ReturnsAllSectionsInOneRequest_AndRequiresAuth()
    {
        var unauth = await factory.CreateClient().GetAsync("/api/v1/dashboard/student");
        Assert.Equal(HttpStatusCode.Unauthorized, unauth.StatusCode);

        var (client, studentId, _, _) = await _api.RegisterAsync("dash");
        var res = await client.GetAsync("/api/v1/dashboard/student");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var dto = await ApiHelper.DataAsync<StudentDashboardDto>(res);
        Assert.Equal(studentId, dto.Score.StudentId);
        Assert.NotNull(dto.MyRegistrations);
        Assert.NotNull(dto.MyApplications);
        Assert.NotNull(dto.Notifications);
        Assert.True(dto.UnreadNotificationsCount >= 0);
        Assert.NotNull(dto.RecruitingProjects);
    }

    [Fact]
    public async Task ExploreFeed_IsPublicAndIncludesProjectsOnlyWhenSignedIn()
    {
        var anonymous = await factory.CreateClient().GetAsync("/api/v1/explore?pageSize=6");
        Assert.Equal(HttpStatusCode.OK, anonymous.StatusCode);
        var publicFeed = await ApiHelper.DataAsync<ExploreFeedDto>(anonymous);
        Assert.Empty(publicFeed.Projects.Items);

        var (client, _, _, _) = await _api.RegisterAsync("explore");
        await _api.CreateProjectAsync(client);
        var authed = await client.GetAsync("/api/v1/explore?pageSize=6");
        var feed = await ApiHelper.DataAsync<ExploreFeedDto>(authed);
        Assert.NotEmpty(feed.Projects.Items);
    }

    [Fact]
    public async Task GamificationHub_ReturnsScoreCatalogBadgesAndLedger()
    {
        var (client, studentId, _, _) = await _api.RegisterAsync("hub");

        var res = await client.GetAsync("/api/v1/gamification/hub?ledgerPageSize=15");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var dto = await ApiHelper.DataAsync<GamificationHubDto>(res);
        Assert.Equal(studentId, dto.Score.StudentId);
        Assert.NotNull(dto.BadgeCatalog);
        Assert.NotNull(dto.EarnedBadges);
        Assert.NotNull(dto.Ledger);
    }

    [Fact]
    public async Task StudentPortfolio_ReturnsProfileSkillsScoreAndBadges_WithEmailPrivacyMasking()
    {
        var (ownerClient, ownerId, _, _) = await _api.RegisterAsync("portfolioowner");
        var (peerClient, _, _, _) = await _api.RegisterAsync("portfoliopeer");

        var res = await peerClient.GetAsync($"/api/v1/students/{ownerId}/portfolio");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var dto = await ApiHelper.DataAsync<StudentPortfolioDto>(res);
        Assert.Equal(ownerId, dto.Profile.StudentId);
        Assert.Equal(ownerId, dto.Score.StudentId);
        Assert.Null(dto.Profile.Email); // Private by default to peers
        Assert.NotNull(dto.Skills);
        Assert.NotNull(dto.Badges);

        var ownRes = await ownerClient.GetAsync($"/api/v1/students/{ownerId}/portfolio");
        Assert.Equal(HttpStatusCode.OK, ownRes.StatusCode);
        var ownDto = await ApiHelper.DataAsync<StudentPortfolioDto>(ownRes);
        Assert.NotNull(ownDto.Profile.Email);

        var notFound = await peerClient.GetAsync("/api/v1/students/999999/portfolio");
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task EventAndSessionWorkspaces_EnforceOrganizerBolaAndReturnRegistrationState()
    {
        var (adminClient, _) = await RegisterRoleAsync("evadmin", nameof(UserRole.OrgAdmin));
        var (studentClient, studentId, _, _) = await _api.RegisterAsync("evstud");
        var (_, ev, ses) = await CreateOrgEventAndSessionAsync(adminClient);

        // Register student for the event
        var regRes = await studentClient.PostAsync($"/api/v1/events/{ev.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);

        // Regular student -> CanManage = false, Registrations = null, MyRegistration populated
        var studWsRes = await studentClient.GetAsync($"/api/v1/events/{ev.EventId}/workspace");
        Assert.Equal(HttpStatusCode.OK, studWsRes.StatusCode);
        var studWs = await ApiHelper.DataAsync<EventWorkspaceDto>(studWsRes);
        Assert.False(studWs.CanManage);
        Assert.Null(studWs.Registrations);
        Assert.NotNull(studWs.MyRegistration);
        Assert.Equal(ev.EventId, studWs.MyRegistration.EventId);
        Assert.Single(studWs.Event.Sessions);

        // OrgAdmin -> CanManage = true, Registrations populated
        var adminWsRes = await adminClient.GetAsync($"/api/v1/events/{ev.EventId}/workspace");
        Assert.Equal(HttpStatusCode.OK, adminWsRes.StatusCode);
        var adminWs = await ApiHelper.DataAsync<EventWorkspaceDto>(adminWsRes);
        Assert.True(adminWs.CanManage);
        Assert.NotNull(adminWs.Registrations);
        Assert.Contains(adminWs.Registrations, r => r.StudentId == studentId);

        // Session attendance workspace: student does NOT receive attendance/registrations
        var studAttWsRes = await studentClient.GetAsync($"/api/v1/events/{ev.EventId}/sessions/{ses.SessionId}/workspace");
        Assert.Equal(HttpStatusCode.OK, studAttWsRes.StatusCode);
        var studAttWs = await ApiHelper.DataAsync<SessionAttendanceWorkspaceDto>(studAttWsRes);
        Assert.False(studAttWs.CanManage);
        Assert.Null(studAttWs.Attendance);
        Assert.Null(studAttWs.Registrations);
        Assert.Equal(ses.SessionId, studAttWs.Session?.SessionId);

        // Session attendance workspace: OrgAdmin gets session + attendance + registrations
        var adminAttWsRes = await adminClient.GetAsync($"/api/v1/events/{ev.EventId}/sessions/{ses.SessionId}/workspace");
        Assert.Equal(HttpStatusCode.OK, adminAttWsRes.StatusCode);
        var adminAttWs = await ApiHelper.DataAsync<SessionAttendanceWorkspaceDto>(adminAttWsRes);
        Assert.True(adminAttWs.CanManage);
        Assert.NotNull(adminAttWs.Attendance);
        Assert.NotNull(adminAttWs.Registrations);
        Assert.Equal(ses.SessionId, adminAttWs.Session?.SessionId);

        var notFound = await studentClient.GetAsync("/api/v1/events/999999/workspace");
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task ProjectWorkspace_EnforcesCreatorAndMemberVisibility()
    {
        var (creatorClient, _, _, _) = await _api.RegisterAsync("projcreator");
        var (applicantClient, applicantId, _, _) = await _api.RegisterAsync("projapplicant");
        var project = await _api.CreateProjectAsync(creatorClient);
        await _api.CreateTaskAsync(creatorClient, project.ProjectId, "Seed Workspace Task");

        var applyRes = await applicantClient.PostAsJsonAsync(
            $"/api/v1/projects/{project.ProjectId}/applications",
            new CreateProjectApplicationRequest("I would like to join."), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, applyRes.StatusCode);

        // Non-member applicant gets MyApplication only, no Tasks, no Applications list, no AvailableSkills
        var appWsRes = await applicantClient.GetAsync($"/api/v1/projects/{project.ProjectId}/workspace");
        Assert.Equal(HttpStatusCode.OK, appWsRes.StatusCode);
        var appWs = await ApiHelper.DataAsync<ProjectWorkspaceDto>(appWsRes);
        Assert.False(appWs.IsCreator);
        Assert.False(appWs.IsMember);
        Assert.Null(appWs.Applications);
        Assert.Null(appWs.Tasks);
        Assert.Null(appWs.AvailableSkills);
        Assert.NotNull(appWs.MyApplication);
        Assert.Equal(applicantId, appWs.MyApplication.StudentId);

        // Creator gets full Applications, Tasks, and AvailableSkills
        var creatorWsRes = await creatorClient.GetAsync($"/api/v1/projects/{project.ProjectId}/workspace");
        Assert.Equal(HttpStatusCode.OK, creatorWsRes.StatusCode);
        var creatorWs = await ApiHelper.DataAsync<ProjectWorkspaceDto>(creatorWsRes);
        Assert.True(creatorWs.IsCreator);
        Assert.True(creatorWs.IsMember);
        Assert.NotNull(creatorWs.Applications);
        Assert.Single(creatorWs.Applications);
        Assert.Null(creatorWs.MyApplication);
        Assert.NotNull(creatorWs.Tasks);
        Assert.Single(creatorWs.Tasks);
        Assert.NotNull(creatorWs.AvailableSkills);
    }

    [Fact]
    public async Task OrganizationWorkspaceAndOrgAdminDashboard_EnforceScopedLeadership()
    {
        var (adminClient, _) = await RegisterRoleAsync("orgwsadmin", nameof(UserRole.OrgAdmin));
        var (studentClient, _, _, _) = await _api.RegisterAsync("orgwsstud");
        var (org, _, _) = await CreateOrgEventAndSessionAsync(adminClient);

        // Student applies to org
        var applyRes = await studentClient.PostAsJsonAsync(
            $"/api/v1/organizations/{org.OrganizationId}/applications",
            new CreateMembershipApplicationRequest("Excited to join!"), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.Created, applyRes.StatusCode);

        // Student org workspace -> CanManage=false, AdminApplications=null, MyApplications populated
        var studWsRes = await studentClient.GetAsync($"/api/v1/organizations/{org.OrganizationId}/workspace");
        Assert.Equal(HttpStatusCode.OK, studWsRes.StatusCode);
        var studWs = await ApiHelper.DataAsync<OrganizationWorkspaceDto>(studWsRes);
        Assert.False(studWs.CanManage);
        Assert.False(studWs.CanReviewRecruitment);
        Assert.Null(studWs.AdminApplications);
        Assert.NotNull(studWs.MyApplications);
        Assert.Contains(studWs.MyApplications, a => a.OrganizationId == org.OrganizationId);

        // OrgAdmin org workspace -> CanManage=true, AdminApplications populated
        var adminWsRes = await adminClient.GetAsync($"/api/v1/organizations/{org.OrganizationId}/workspace");
        Assert.Equal(HttpStatusCode.OK, adminWsRes.StatusCode);
        var adminWs = await ApiHelper.DataAsync<OrganizationWorkspaceDto>(adminWsRes);
        Assert.True(adminWs.CanManage);
        Assert.True(adminWs.CanReviewRecruitment);
        Assert.NotNull(adminWs.AdminApplications);
        Assert.NotEmpty(adminWs.AdminApplications);
        Assert.Null(adminWs.MyApplications);

        // OrgAdmin dashboard -> single request returns managed org + apps + hosted events
        var orgDashRes = await adminClient.GetAsync($"/api/v1/dashboard/org?organizationId={org.OrganizationId}");
        Assert.Equal(HttpStatusCode.OK, orgDashRes.StatusCode);
        var orgDash = await ApiHelper.DataAsync<OrgAdminDashboardDto>(orgDashRes);
        Assert.Equal(org.OrganizationId, orgDash.ActiveOrganizationId);
        Assert.Contains(orgDash.ManagedOrganizations, o => o.OrganizationId == org.OrganizationId);
        Assert.NotEmpty(orgDash.Applications);
        Assert.NotEmpty(orgDash.HostedEvents.Items);

        // Plain student -> empty managed orgs without leaking applications
        var studDashRes = await studentClient.GetAsync("/api/v1/dashboard/org");
        Assert.Equal(HttpStatusCode.OK, studDashRes.StatusCode);
        var studDash = await ApiHelper.DataAsync<OrgAdminDashboardDto>(studDashRes);
        Assert.Empty(studDash.ManagedOrganizations);
        Assert.Null(studDash.ActiveOrganizationId);
        Assert.Empty(studDash.Applications);
    }
}
