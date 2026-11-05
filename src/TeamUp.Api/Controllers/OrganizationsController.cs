using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Organizations;

namespace TeamUp.Api.Controllers;

[Route("api/v1/organizations")]
[Authorize]
public sealed class OrganizationsController(IOrganizationService organizationService) : BaseApiController
{
    [HttpGet]
    [ProducesOk<PagedResult<OrganizationSummaryDto>>]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] string? search, [FromQuery] string? category,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await organizationService.GetOrganizationsAsync(search, category, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesCreated<OrganizationDetailDto>]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request, CancellationToken ct)
    {
        var result = await organizationService.CreateOrganizationAsync(request, ct);
        return CreatedResponse(nameof(GetOrganizationById), new { id = result.OrganizationId }, result, "Organization created successfully.");
    }

    [HttpGet("{id:int}")]
    [ProducesOk<OrganizationDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetOrganizationById([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetOrganizationByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesOk<OrganizationDetailDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateOrganization([FromRoute] int id, [FromBody] UpdateOrganizationRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateOrganizationAsync(id, request, ct);
        return OkResponse(result, "Organization updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteOrganization([FromRoute] int id, CancellationToken ct)
    {
        await organizationService.DeleteOrganizationAsync(id, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/members")]
    [ProducesOk<IReadOnlyList<OrganizationMemberDto>>, ProducesNotFound]
    public async Task<IActionResult> GetMembers([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetMembersAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}/members/{studentId:int}")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesOk<OrganizationMemberDto>]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> UpdateMemberRole(
        [FromRoute] int id, [FromRoute] int studentId,
        [FromBody] UpdateOrganizationMemberRequest request, CancellationToken ct)
    {
        var result = await organizationService.UpdateMemberRoleAsync(id, studentId, request, ct);
        return OkResponse(result, "Organization member role updated successfully.");
    }

    [HttpDelete("{id:int}/members/{studentId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    {
        if (CurrentStudentId != studentId && CurrentUserRole is not ("OrgAdmin" or "PlatformAdmin"))
            return Forbid();

        await organizationService.RemoveMemberAsync(id, studentId, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/applications")]
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesOk<IReadOnlyList<MembershipApplicationDto>>, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> GetApplications([FromRoute] int id, CancellationToken ct)
    {
        var result = await organizationService.GetApplicationsAsync(id, ct);
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
    [Authorize(Roles = "OrgAdmin,PlatformAdmin")]
    [ProducesOk<MembershipApplicationDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ReviewApplication(
        [FromRoute] int id, [FromRoute] int appId,
        [FromBody] ReviewMembershipApplicationRequest request, CancellationToken ct)
    {
        var result = await organizationService.ReviewApplicationAsync(id, appId, request, ct);
        return OkResponse(result, "Membership application reviewed successfully.");
    }

    [HttpDelete("{id:int}/applications/{appId:int}")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> WithdrawApplication([FromRoute] int id, [FromRoute] int appId, CancellationToken ct)
    {
        await organizationService.WithdrawApplicationAsync(id, appId, CurrentStudentId, ct);
        return NoContentResponse();
    }
}
