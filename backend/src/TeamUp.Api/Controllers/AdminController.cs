// Date: 2026-11-21
// Purpose: Platform administration endpoints for system metrics, guarded role updates, universal soft-delete recovery, and immutable audit logs (STRIDE THR-R-01, THR-E-03).

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Enums;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Dedicated cross-cutting administration dashboard, role governance, audit logs, governance PDF reports, email template previews, and soft-delete recovery.
/// </summary>
[Route("api/v1/admin")]
[Authorize(Roles = "PlatformAdmin")]
[ProducesForbidden]
public class AdminController(
    IAdminService adminService,
    IOrganizationService organizationService,
    IAdminReportService reportService,
    IDigestDispatchService digestService,
    ITemplateRenderer templateRenderer,
    IAuditLogService auditLog) : BaseApiController
{
    [HttpGet("dashboard")]
    [ProducesOk<AdminDashboardDto>]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await adminService.GetDashboardStatsAsync(ct);
        return OkResponse(result);
    }

    [HttpPatch("students/{id:int}/role")]
    [ProducesOk<StudentSummaryDto>, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateStudentRole(
        [FromRoute] int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        var result = await adminService.UpdateStudentRoleAsync(id, request.Role, CurrentStudentId, ct);
        return OkResponse(result, "Student role updated successfully.");
    }

    [HttpGet("soft-deleted")]
    [ProducesOk<IReadOnlyList<SoftDeletedItemDto>>]
    public async Task<IActionResult> GetSoftDeleted([FromQuery] string? entityType, CancellationToken ct)
    {
        var result = await adminService.GetSoftDeletedRecordsAsync(entityType, ct);
        return OkResponse(result);
    }

    [HttpPost("restore/{entityType}/{id:int}")]
    [ProducesOk, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> RestoreEntity(
        [FromRoute] string entityType,
        [FromRoute] int id,
        CancellationToken ct)
    {
        await adminService.RestoreEntityAsync(entityType, id, ct);
        return OkMessage($"{entityType} with ID {id} has been restored successfully.");
    }

    [HttpGet("audit-logs")]
    [ProducesOk<PagedResult<AuditLogDto>>]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] int? actorStudentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await adminService.GetAuditLogsAsync(action, entityType, actorStudentId, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpGet("reports/platform-summary.pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadPlatformSummaryPdf(
        [FromQuery] string? reportPeriod = null,
        CancellationToken ct = default)
    {
        var report = await reportService.GeneratePlatformSummaryPdfAsync(CurrentStudentId, reportPeriod, ct);
        return File(report.PdfBytes, "application/pdf", report.Summary.FileName);
    }

    [HttpPost("reports/send-summary")]
    [ProducesOk<AdminReportEmailResultDto>]
    public async Task<IActionResult> SendPlatformSummaryEmail(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SendAdminReportRequest? request = null,
        CancellationToken ct = default)
    {
        var result = await reportService.SendPlatformSummaryEmailAsync(
            CurrentStudentId,
            request?.RecipientEmail,
            request?.ReportPeriod,
            ct);
        return OkResponse(result, "Governance PDF report dispatched.");
    }

    [HttpGet("templates")]
    [ProducesOk<IReadOnlyList<string>>]
    public IActionResult ListEmailTemplates() =>
        OkResponse(templateRenderer.GetAvailableEmailTemplates());

    [HttpGet("templates/preview/{templateName}")]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesNotFound]
    public async Task<IActionResult> PreviewEmailTemplate(
        [FromRoute] string templateName,
        CancellationToken ct = default)
    {
        var html = await reportService.RenderTemplatePreviewAsync(templateName, ct);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("communications/leaderboard-digest")]
    [ProducesOk<DigestDispatchResultDto>]
    public async Task<IActionResult> DispatchLeaderboardDigest(
        [FromBody] AdminLeaderboardDigestCommand request,
        CancellationToken ct = default)
    {
        var isTest = request.Mode.Equals("TestToMe", StringComparison.OrdinalIgnoreCase);
        var testEmail = isTest ? ResolveActorEmail() : null;

        var result = await digestService.DispatchLeaderboardDigestAsync(
            new LeaderboardDigestRequest(request.PeriodLabel, request.NewsItems, request.DryRun, testEmail),
            ct);

        await auditLog.RecordAsync(
            "LeaderboardDigestDispatched",
            "Communication",
            request.Mode,
            newValues: new { result.IsTestMode, result.DryRun, result.EligibleRecipientsCount, result.SentCount },
            actorStudentIdOverride: CurrentStudentId > 0 ? CurrentStudentId : null,
            ct: ct);

        return OkResponse(result, "Leaderboard digest processed.");
    }

    [HttpPost("communications/project-matches")]
    [ProducesOk<DigestDispatchResultDto>]
    public async Task<IActionResult> DispatchProjectMatchDigest(
        [FromBody] AdminProjectMatchDigestCommand request,
        CancellationToken ct = default)
    {
        var isTest = request.Mode.Equals("TestToMe", StringComparison.OrdinalIgnoreCase);
        var testEmail = isTest ? ResolveActorEmail() : null;

        var result = await digestService.DispatchProjectMatchDigestAsync(
            new ProjectMatchDigestRequest(request.MaxMatchesPerStudent, request.MinMatchPercentage, request.DryRun, testEmail),
            ct);

        await auditLog.RecordAsync(
            "ProjectMatchDigestDispatched",
            "Communication",
            request.Mode,
            newValues: new { result.IsTestMode, result.DryRun, result.EligibleRecipientsCount, result.SentCount, result.SkippedCount },
            actorStudentIdOverride: CurrentStudentId > 0 ? CurrentStudentId : null,
            ct: ct);

        return OkResponse(result, "Project match digest processed.");
    }

    [HttpGet("organization-requests")]
    [ProducesOk<PagedResult<OrganizationCreationRequestDto>>]
    public async Task<IActionResult> GetOrganizationRequests(
        [FromQuery] ApplicationStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await organizationService.GetAdminOrganizationCreationRequestsAsync(
            status, search, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpPatch("organization-requests/{requestId:int}/review")]
    [ProducesOk<OrganizationCreationRequestDto>, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ReviewOrganizationRequest(
        [FromRoute] int requestId,
        [FromBody] ReviewOrganizationProposalRequest request,
        CancellationToken ct = default)
    {
        var result = await organizationService.ReviewOrganizationCreationRequestAsync(
            requestId, CurrentStudentId, request, ct);
        var message = request.Approve
            ? "Organization approved and requester promoted to Organization Admin."
            : "Organization request rejected.";
        return OkResponse(result, message);
    }

    private string ResolveActorEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? "admin@teamup.local";
}
