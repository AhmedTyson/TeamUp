// Date: 2026-11-27
// Purpose: Composite single-request page endpoints (dashboards, explore feed, event/session/project/org workspaces,
// gamification hub, student portfolio). Each delegates to existing services so authorization and privacy rules remain intact.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Workspaces;

namespace TeamUp.Api.Controllers;

[Route("api/v1")]
[Authorize]
public sealed class WorkspacesController(IWorkspaceService workspaces) : BaseApiController
{
    #region Dashboards & Explore

    [HttpGet("dashboard/student")]
    [ProducesOk<StudentDashboardDto>]
    public async Task<IActionResult> GetStudentDashboard(CancellationToken ct)
        => OkResponse(await workspaces.GetStudentDashboardAsync(CurrentStudentId, ct));

    [HttpGet("dashboard/org")]
    [ProducesOk<OrgAdminDashboardDto>]
    public async Task<IActionResult> GetOrgAdminDashboard([FromQuery] int? organizationId = null, CancellationToken ct = default)
        => OkResponse(await workspaces.GetOrgAdminDashboardAsync(CurrentStudentId, CurrentUserRole, organizationId, ct));

    [HttpGet("explore")]
    [AllowAnonymous]
    [ProducesOk<ExploreFeedDto>]
    public async Task<IActionResult> GetExploreFeed([FromQuery] int pageSize = 6, CancellationToken ct = default)
        => OkResponse(await workspaces.GetExploreFeedAsync(pageSize, User.Identity?.IsAuthenticated == true, ct));

    #endregion

    #region Detail Workspaces

    [HttpGet("events/{id:int}/workspace")]
    [ProducesOk<EventWorkspaceDto>, ProducesNotFound]
    public async Task<IActionResult> GetEventWorkspace([FromRoute] int id, CancellationToken ct)
        => OkResponse(await workspaces.GetEventWorkspaceAsync(id, CurrentStudentId, CurrentUserRole, ct));

    [HttpGet("events/{eventId:int}/sessions/{sessionId:int}/workspace")]
    [ProducesOk<SessionAttendanceWorkspaceDto>, ProducesNotFound]
    public async Task<IActionResult> GetSessionAttendanceWorkspace(
        [FromRoute] int eventId, [FromRoute] int sessionId, CancellationToken ct)
        => OkResponse(await workspaces.GetSessionAttendanceWorkspaceAsync(eventId, sessionId, CurrentStudentId, CurrentUserRole, ct));

    [HttpGet("projects/{id:int}/workspace")]
    [ProducesOk<ProjectWorkspaceDto>, ProducesNotFound]
    public async Task<IActionResult> GetProjectWorkspace([FromRoute] int id, CancellationToken ct)
        => OkResponse(await workspaces.GetProjectWorkspaceAsync(id, CurrentStudentId, ct));

    [HttpGet("organizations/{id:int}/workspace")]
    [ProducesOk<OrganizationWorkspaceDto>, ProducesNotFound]
    public async Task<IActionResult> GetOrganizationWorkspace([FromRoute] int id, CancellationToken ct)
        => OkResponse(await workspaces.GetOrganizationWorkspaceAsync(id, CurrentStudentId, CurrentUserRole, ct));

    #endregion

    #region Gamification & Student Portfolio

    [HttpGet("gamification/hub")]
    [ProducesOk<GamificationHubDto>]
    public async Task<IActionResult> GetGamificationHub([FromQuery] int ledgerPageSize = 15, CancellationToken ct = default)
        => OkResponse(await workspaces.GetGamificationHubAsync(CurrentStudentId, Math.Clamp(ledgerPageSize, 1, 50), ct));

    [HttpGet("students/{id:int}/portfolio")]
    [ProducesOk<StudentPortfolioDto>, ProducesNotFound]
    public async Task<IActionResult> GetStudentPortfolio([FromRoute] int id, CancellationToken ct)
        => OkResponse(await workspaces.GetStudentPortfolioAsync(id, CurrentStudentId, CurrentUserRole, ct));

    #endregion
}
