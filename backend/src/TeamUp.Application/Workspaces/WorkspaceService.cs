// Date: 2026-11-27
// Purpose: Composes existing feature services into single-request page payloads. Every part reuses the granular
// service (and therefore its authorization/privacy rules); queries run sequentially because the scoped DbContext
// is not thread-safe.

using TeamUp.Application.Common;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Skills;
using TeamUp.Application.Students;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Workspaces;

public interface IWorkspaceService
{
    Task<EventWorkspaceDto> GetEventWorkspaceAsync(int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<SessionAttendanceWorkspaceDto> GetSessionAttendanceWorkspaceAsync(int eventId, int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<ProjectWorkspaceDto> GetProjectWorkspaceAsync(int projectId, int actorStudentId, CancellationToken ct = default);
    Task<OrganizationWorkspaceDto> GetOrganizationWorkspaceAsync(int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId, CancellationToken ct = default);
    Task<OrgAdminDashboardDto> GetOrgAdminDashboardAsync(int actorStudentId, string? actorRole, int? requestedOrganizationId = null, CancellationToken ct = default);
    Task<ExploreFeedDto> GetExploreFeedAsync(int pageSize, bool authenticated, CancellationToken ct = default);
    Task<GamificationHubDto> GetGamificationHubAsync(int studentId, int ledgerPageSize, CancellationToken ct = default);
    Task<StudentPortfolioDto> GetStudentPortfolioAsync(int studentId, int viewerStudentId, string? viewerRole, CancellationToken ct = default);
}

public sealed class WorkspaceService(
    IEventService events,
    IOrganizationService organizations,
    IOrganizationAuthorizationService orgAuth,
    IProjectService projects,
    IProjectMemberService projectMembers,
    IProjectTaskService projectTasks,
    IGamificationService gamification,
    IBadgeService badges,
    INotificationService notifications,
    IStudentAccountService students,
    ISkillService skills) : IWorkspaceService
{
    private const int DashboardNotificationTake = 15;
    private const int DashboardRegistrationsPageSize = 20;
    private const int DashboardProjectsPageSize = 4;
    private const int OrgDashboardManagedPageSize = 50;
    private const int OrgDashboardEventsPageSize = 6;

    #region Event & Attendance Workspaces

    public async Task<EventWorkspaceDto> GetEventWorkspaceAsync(
        int eventId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var evt = await events.GetEventByIdAsync(eventId, actorStudentId, actorRole, ct);

        var isPlatformAdmin = string.Equals(actorRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase);
        var canManage = isPlatformAdmin || await orgAuth.IsOrgAdminAsync(evt.OrganizationId, actorStudentId, ct);

        var myRegistration = await events.GetMyRegistrationForEventAsync(eventId, actorStudentId, ct);
        var registrations = canManage
            ? await events.GetRegistrationsAsync(eventId, actorStudentId, actorRole, ct)
            : null;

        return new EventWorkspaceDto(evt, canManage, myRegistration, registrations);
    }

    public async Task<SessionAttendanceWorkspaceDto> GetSessionAttendanceWorkspaceAsync(
        int eventId, int sessionId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var workspace = await GetEventWorkspaceAsync(eventId, actorStudentId, actorRole, ct);
        var session = workspace.Event.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        var attendance = workspace.CanManage && session is not null
            ? await events.GetAttendanceAsync(sessionId, actorStudentId, actorRole, ct)
            : null;

        return new SessionAttendanceWorkspaceDto(
            workspace.Event,
            session,
            workspace.CanManage,
            workspace.Registrations,
            attendance);
    }

    #endregion

    #region Project & Organization Workspaces

    public async Task<ProjectWorkspaceDto> GetProjectWorkspaceAsync(
        int projectId, int actorStudentId, CancellationToken ct = default)
    {
        var project = await projects.GetProjectAsync(projectId, ct);
        var isCreator = project.CreatorId == actorStudentId;
        var isMember = project.Members.Any(m => m.StudentId == actorStudentId);

        var applications = isCreator
            ? await projectMembers.GetApplicationsAsync(actorStudentId, projectId, ct)
            : null;
        var myApplication = !isCreator
            ? await projectMembers.GetMyApplicationAsync(actorStudentId, projectId, ct)
            : null;
        var tasks = isMember
            ? await projectTasks.GetProjectTasksAsync(actorStudentId, projectId, ct)
            : null;
        var mySkills = await skills.GetStudentSkillsAsync(actorStudentId, ct);
        var availableSkills = isCreator
            ? await skills.GetSkillsAsync(null, null, ct)
            : null;

        return new ProjectWorkspaceDto(
            project,
            isCreator,
            isMember,
            applications,
            myApplication,
            tasks,
            mySkills,
            availableSkills);
    }

    public async Task<OrganizationWorkspaceDto> GetOrganizationWorkspaceAsync(
        int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default)
    {
        var org = await organizations.GetOrganizationByIdAsync(organizationId, actorStudentId, actorRole, ct);
        var isPlatformAdmin = string.Equals(actorRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase);
        var canManage = isPlatformAdmin || await orgAuth.IsOrgAdminAsync(organizationId, actorStudentId, ct);
        var canReviewRecruitment = canManage || await orgAuth.CanReviewRecruitmentAsync(
            organizationId, actorStudentId, actorRole, targetUnitId: null, ct);
        var isMember = org.Members.Any(m => m.StudentId == actorStudentId);

        var adminApps = canReviewRecruitment
            ? await organizations.GetApplicationsAsync(organizationId, actorStudentId, actorRole, ct)
            : null;
        var myApps = !canReviewRecruitment
            ? await organizations.GetMyApplicationsAsync(actorStudentId, ct)
            : null;

        return new OrganizationWorkspaceDto(
            org,
            canManage,
            canReviewRecruitment,
            isMember,
            adminApps,
            myApps);
    }

    #endregion

    #region Dashboards

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId, CancellationToken ct = default)
    {
        var score = await gamification.GetStudentScoreAsync(studentId, ct);
        var registrations = await events.GetMyRegistrationsAsync(
            studentId, new PageRequest(1, DashboardRegistrationsPageSize), ct);
        var applications = await organizations.GetMyApplicationsAsync(studentId, ct);
        var inbox = await notifications.GetNotificationsAsync(studentId, false, DashboardNotificationTake, ct);
        var unread = await notifications.GetUnreadCountAsync(studentId, ct);
        var recruiting = await projects.GetProjectsAsync(
            null, nameof(ProjectStatus.Recruiting), null, new PageRequest(1, DashboardProjectsPageSize), ct);

        return new StudentDashboardDto(score, registrations, applications, inbox, unread, recruiting);
    }

    public async Task<OrgAdminDashboardDto> GetOrgAdminDashboardAsync(
        int actorStudentId, string? actorRole, int? requestedOrganizationId = null, CancellationToken ct = default)
    {
        var emptyEvents = new PagedResult<EventDto>([], 1, OrgDashboardEventsPageSize, 0);
        var isPlatformAdmin = string.Equals(actorRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase);
        var adminOrgIds = isPlatformAdmin
            ? []
            : await orgAuth.GetAdminOrganizationIdsAsync(actorStudentId, ct);

        if (!isPlatformAdmin && adminOrgIds.Count == 0)
            return new OrgAdminDashboardDto([], null, [], emptyEvents);

        var allOrgs = (await organizations.GetOrganizationsAsync(
            null, null, new PageRequest(1, OrgDashboardManagedPageSize), ct: ct)).Items;

        var managedOrgs = isPlatformAdmin
            ? allOrgs
            : allOrgs.Where(o => adminOrgIds.Contains(o.OrganizationId)).ToList();

        if (managedOrgs.Count == 0)
            return new OrgAdminDashboardDto([], null, [], emptyEvents);

        var activeOrgId = requestedOrganizationId.HasValue && managedOrgs.Any(o => o.OrganizationId == requestedOrganizationId.Value)
            ? requestedOrganizationId.Value
            : managedOrgs[0].OrganizationId;

        var applications = await organizations.GetApplicationsAsync(activeOrgId, actorStudentId, actorRole, ct);
        var hostedEvents = await events.GetEventsAsync(
            null, activeOrgId, new PageRequest(1, OrgDashboardEventsPageSize), ct: ct);

        return new OrgAdminDashboardDto(managedOrgs, activeOrgId, applications, hostedEvents);
    }

    #endregion

    #region Explore Feed

    public async Task<ExploreFeedDto> GetExploreFeedAsync(int pageSize, bool authenticated, CancellationToken ct = default)
    {
        var page = new PageRequest(1, Math.Clamp(pageSize, 1, 12));

        var orgs = await organizations.GetOrganizationsAsync(null, null, page, null, null, true, ct);
        var upcoming = authenticated
            ? await events.GetEventsAsync(null, null, page, ct: ct)
            : await events.GetPublicEventsAsync(null, null, page, ct: ct);
        var projectPage = authenticated
            ? await projects.GetProjectsAsync(null, null, null, page, ct)
            : new PagedResult<ProjectListItemDto>([], page.SafePage, page.SafePageSize, 0);

        return new ExploreFeedDto(orgs, upcoming, projectPage);
    }

    #endregion

    #region Gamification Hub & Student Portfolio

    public async Task<GamificationHubDto> GetGamificationHubAsync(
        int studentId, int ledgerPageSize, CancellationToken ct = default)
    {
        var score = await gamification.GetStudentScoreAsync(studentId, ct);
        var catalog = await badges.GetBadgeCatalogAsync(ct);
        var earned = await badges.GetStudentBadgesAsync(studentId, ct);
        var ledger = await gamification.GetScoreTransactionsAsync(studentId, new PageRequest(1, ledgerPageSize), ct);
        return new GamificationHubDto(score, catalog, earned, ledger);
    }

    public async Task<StudentPortfolioDto> GetStudentPortfolioAsync(
        int studentId, int viewerStudentId, string? viewerRole, CancellationToken ct = default)
    {
        var profile = await students.GetStudentPublicProfileByIdAsync(studentId, viewerStudentId, viewerRole, ct);
        var studentSkills = await skills.GetStudentSkillsAsync(studentId, ct);
        var score = await gamification.GetStudentScoreAsync(studentId, ct);
        var earned = await badges.GetStudentBadgesAsync(studentId, ct);
        return new StudentPortfolioDto(profile, studentSkills, score, earned);
    }

    #endregion
}
