// Date: 2026-11-21
// Purpose: Organization service contract with actor-scoped multi-tenant authorization parameters (STRIDE THR-T-01, THR-E-01).

using TeamUp.Application.Common;

namespace TeamUp.Application.Organizations;

public interface IOrganizationService
{
    Task<PagedResult<OrganizationSummaryDto>> GetOrganizationsAsync(string? search, string? category, PageRequest page, CancellationToken ct = default);
    Task<OrganizationDetailDto> GetOrganizationByIdAsync(int id, CancellationToken ct = default);
    Task<OrganizationDetailDto> CreateOrganizationAsync(int creatorStudentId, CreateOrganizationRequest request, CancellationToken ct = default);
    Task<OrganizationDetailDto> UpdateOrganizationAsync(int id, int actorStudentId, string? actorRole, UpdateOrganizationRequest request, CancellationToken ct = default);
    Task DeleteOrganizationAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationMemberDto>> GetMembersAsync(int organizationId, CancellationToken ct = default);
    Task<OrganizationMemberDto> UpdateMemberRoleAsync(int organizationId, int studentId, int actorStudentId, string? actorRole, UpdateOrganizationMemberRequest request, CancellationToken ct = default);
    Task RemoveMemberAsync(int organizationId, int studentId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipApplicationDto>> GetApplicationsAsync(int organizationId, int actorStudentId, string? actorRole, CancellationToken ct = default);
    Task<MembershipApplicationDto> ApplyForMembershipAsync(int organizationId, int studentId, CreateMembershipApplicationRequest request, CancellationToken ct = default);
    Task<MembershipApplicationDto> ReviewApplicationAsync(int organizationId, int applicationId, int actorStudentId, string? actorRole, ReviewMembershipApplicationRequest request, CancellationToken ct = default);
    Task WithdrawApplicationAsync(int organizationId, int applicationId, int studentId, CancellationToken ct = default);
}
