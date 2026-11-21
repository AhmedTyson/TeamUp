using TeamUp.Application.Common;

namespace TeamUp.Application.Organizations;

public interface IOrganizationService
{
    Task<PagedResult<OrganizationSummaryDto>> GetOrganizationsAsync(string? search, string? category, PageRequest page, CancellationToken ct = default);
    Task<OrganizationDetailDto> GetOrganizationByIdAsync(int id, CancellationToken ct = default);
    Task<OrganizationDetailDto> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken ct = default);
    Task<OrganizationDetailDto> UpdateOrganizationAsync(int id, UpdateOrganizationRequest request, CancellationToken ct = default);
    Task DeleteOrganizationAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationMemberDto>> GetMembersAsync(int organizationId, CancellationToken ct = default);
    Task<OrganizationMemberDto> UpdateMemberRoleAsync(int organizationId, int studentId, UpdateOrganizationMemberRequest request, CancellationToken ct = default);
    Task RemoveMemberAsync(int organizationId, int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipApplicationDto>> GetApplicationsAsync(int organizationId, CancellationToken ct = default);
    Task<MembershipApplicationDto> ApplyForMembershipAsync(int organizationId, int studentId, CreateMembershipApplicationRequest request, CancellationToken ct = default);
    Task<MembershipApplicationDto> ReviewApplicationAsync(int organizationId, int applicationId, ReviewMembershipApplicationRequest request, CancellationToken ct = default);
    Task WithdrawApplicationAsync(int organizationId, int applicationId, int studentId, CancellationToken ct = default);
}
