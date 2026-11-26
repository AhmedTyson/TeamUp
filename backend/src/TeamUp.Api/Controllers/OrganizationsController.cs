// Date: 2026-11-21
// Purpose: Student organizations and membership endpoints with scoped per-organization BOLA authorization (STRIDE THR-T-01, THR-E-01).

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using TeamUp.Api.Controllers.Common;
using TeamUp.Api.Extensions;
using TeamUp.Application.Common;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Enums;

namespace TeamUp.Api.Controllers;

[Route("api/v1/organizations")]
[Authorize]
public sealed class OrganizationsController(
    IOrganizationService organizationService,
    IOutputCacheStore outputCacheStore) : BaseApiController
{
    #region Public Discovery

    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesOk<PagedResult<OrganizationSummaryDto>>]
    public async Task<IActionResult> GetPublicOrganizations(
        [FromQuery] OrganizationFilterQuery filter,
        CancellationToken ct = default)
    {
        var result = await organizationService.GetOrganizationsAsync(
            filter.Search, filter.Category, filter.ToPageRequest(), filter.Scope, filter.UniversityId, filter.RecruitingOnly, ct);
        return OkResponse(result);
    }

    [HttpGet("public/categories")]
    [AllowAnonymous]
    [OutputCache(PolicyName = OpenApiAndCachingExtensions.CatalogOutputCachePolicyName, Tags = [CatalogOutputCacheTags.OrganizationCategories])]
    [ProducesOk<IReadOnlyList<string>>]
    public async Task<IActionResult> GetPublicOrganizationCategories(CancellationToken ct)
    {
        var result = await organizationService.GetOrganizationCategoriesAsync(ct);
        return OkResponse(result);
    }

    [HttpGet("public/{id:int}")]
    [AllowAnonymous]
    [ProducesOk<OrganizationDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetPublicOrganizationById([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetPublicOrganizationByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpGet("public/{id:int}/hierarchy")]
    [AllowAnonymous]
    [ProducesOk<OrganizationHierarchyDto>, ProducesNotFound]
    public async Task<IActionResult> GetPublicHierarchy([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetHierarchyAsync(id, includeEmails: false, ct: ct);
        return OkResponse(result);
    }

    #endregion

    #region Organization CRUD

    [HttpGet("my-applications")]
    [ProducesOk<IReadOnlyList<MembershipApplicationDto>>]
    public async Task<IActionResult> GetMyApplications(CancellationToken ct)
    {
        var result = await organizationService.GetMyApplicationsAsync(CurrentStudentId, ct);
        return OkResponse(result);
    }

    [HttpGet]
    [ProducesOk<PagedResult<OrganizationSummaryDto>>]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] OrganizationFilterQuery filter,
        CancellationToken ct = default)
    {
        var result = await organizationService.GetOrganizationsAsync(
            filter.Search, filter.Category, filter.ToPageRequest(), filter.Scope, filter.UniversityId, filter.RecruitingOnly, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesCreated<OrganizationDetailDto>]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request, CancellationToken ct)
    {
        var result = await organizationService.CreateOrganizationAsync(CurrentStudentId, request, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.OrganizationCategories, ct);
        return CreatedResponse(nameof(GetOrganizationById), new { id = result.OrganizationId }, result, "Organization created successfully.");
    }

    [HttpGet("{id:int}")]
    [ProducesOk<OrganizationDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetOrganizationById([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetOrganizationByIdAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesOk<OrganizationDetailDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateOrganization([FromRoute] int id, [FromBody] UpdateOrganizationRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateOrganizationAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.OrganizationCategories, ct);
        return OkResponse(result, "Organization updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteOrganization([FromRoute] int id, CancellationToken ct)
    {
        await organizationService.DeleteOrganizationAsync(id, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.OrganizationCategories, ct);
        return NoContentResponse();
    }

    #endregion

    #region Members

    [HttpGet("{id:int}/members")]
    [ProducesOk<IReadOnlyList<OrganizationMemberDto>>, ProducesNotFound]
    public async Task<IActionResult> GetMembers([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetMembersAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}/members/{studentId:int}")]
    [ProducesOk<OrganizationMemberDto>]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> UpdateMemberRole(
        [FromRoute] int id, [FromRoute] int studentId,
        [FromBody] UpdateOrganizationMemberRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateMemberRoleAsync(id, studentId, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Organization member role updated successfully.");
    }

    [HttpDelete("{id:int}/members/{studentId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    {
        await organizationService.RemoveMemberAsync(id, studentId, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }

    #endregion

    #region Membership Applications & Recruitment

    [HttpGet("{id:int}/applications")]
    [ProducesOk<IReadOnlyList<MembershipApplicationDto>>, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> GetApplications([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetApplicationsAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/applications")]
    [ProducesCreated<MembershipApplicationDto>]
    [ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ApplyForMembership(
        [FromRoute] int id, [FromBody] CreateMembershipApplicationRequest request, CancellationToken ct)
    {
        var result = await organizationService.ApplyForMembershipAsync(id, CurrentStudentId, request, ct);
        return CreatedResponse(nameof(GetApplications), new { id }, result, "Membership application submitted successfully.");
    }

    [HttpPatch("{id:int}/applications/{appId:int}")]
    [ProducesOk<MembershipApplicationDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ReviewApplication(
        [FromRoute] int id, [FromRoute] int appId,
        [FromBody] ReviewMembershipApplicationRequest request, CancellationToken ct)
    {
        var result = await organizationService.ReviewApplicationAsync(id, appId, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Membership application reviewed successfully.");
    }

    [HttpDelete("{id:int}/applications/{appId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> WithdrawApplication([FromRoute] int id, [FromRoute] int appId, CancellationToken ct)
    {
        await organizationService.WithdrawApplicationAsync(id, appId, CurrentStudentId, ct);
        return NoContentResponse();
    }

    [HttpPatch("{id:int}/applications/{appId:int}/stage")]
    [ProducesOk<MembershipApplicationDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateApplicationStage(
        [FromRoute] int id, [FromRoute] int appId,
        [FromBody] UpdateApplicationStageRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateApplicationStageAsync(id, appId, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Recruitment stage updated successfully.");
    }

    [HttpGet("{id:int}/applications/export.csv")]
    [Produces("text/csv")]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> ExportApplicationsCsv([FromRoute] int id, CancellationToken ct)
    {
        var csv = await organizationService.ExportApplicationsCsvAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", $"applications-org-{id}.csv");
    }

    #endregion

    #region Units, Hierarchy & Participants

    [HttpGet("{id:int}/units")]
    [ProducesOk<IReadOnlyList<OrganizationUnitDto>>, ProducesNotFound]
    public async Task<IActionResult> GetUnits([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetUnitsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/units")]
    [ProducesCreated<OrganizationUnitDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> CreateUnit(
        [FromRoute] int id, [FromBody] CreateOrganizationUnitRequest request, CancellationToken ct)
    {
        var result = await organizationService.CreateUnitAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        return CreatedResponse(result, "Organization unit created successfully.");
    }

    [HttpPatch("{id:int}/units/{unitId:int}")]
    [ProducesOk<OrganizationUnitDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateUnit(
        [FromRoute] int id, [FromRoute] int unitId, [FromBody] UpdateOrganizationUnitRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateUnitAsync(id, unitId, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Organization unit updated successfully.");
    }

    [HttpDelete("{id:int}/units/{unitId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteUnit([FromRoute] int id, [FromRoute] int unitId, CancellationToken ct)
    {
        await organizationService.DeleteUnitAsync(id, unitId, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/hierarchy")]
    [ProducesOk<OrganizationHierarchyDto>, ProducesNotFound]
    public async Task<IActionResult> GetHierarchy([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetHierarchyAsync(id, includeEmails: true, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:int}/participants")]
    [ProducesOk<IReadOnlyList<OrganizationParticipantDto>>, ProducesNotFound]
    public async Task<IActionResult> GetParticipants([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetParticipantsAsync(id, ct);
        return OkResponse(result);
    }

    #endregion

    #region Creation Proposals

    [HttpPost("requests")]
    [ProducesCreated<OrganizationCreationRequestDto>]
    [ProducesConflict, ProducesNotFound]
    public async Task<IActionResult> SubmitOrganizationCreationRequest(
        [FromBody] CreateOrganizationProposalRequest request, CancellationToken ct)
    {
        var result = await organizationService.SubmitOrganizationCreationRequestAsync(CurrentStudentId, request, ct);
        return CreatedResponse(result, "Organization creation request submitted for PlatformAdmin approval.");
    }

    [HttpGet("requests/my")]
    [ProducesOk<IReadOnlyList<OrganizationCreationRequestDto>>]
    public async Task<IActionResult> GetMyOrganizationCreationRequests(CancellationToken ct)
    {
        var result = await organizationService.GetMyOrganizationCreationRequestsAsync(CurrentStudentId, ct);
        return OkResponse(result);
    }

    [HttpDelete("requests/{requestId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> WithdrawOrganizationCreationRequest([FromRoute] int requestId, CancellationToken ct)
    {
        await organizationService.WithdrawOrganizationCreationRequestAsync(requestId, CurrentStudentId, ct);
        return NoContentResponse();
    }

    #endregion
}
