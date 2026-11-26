// Date: 2026-11-24
// Purpose: Canonical scoped organization roles and Egyptian student-activity hierarchy mapping (STRIDE THR-T-01 / THR-E-01).

using System.Collections.Frozen;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Common;

public static class OrganizationRoles
{
    public const string Admin = "Admin";
    public const string OrgAdmin = "OrgAdmin";
    public const string President = "President";
    public const string VicePresident = "Vice President";
    public const string Chair = "Chair";
    public const string ViceChair = "Vice Chair";
    public const string Secretary = "Secretary";
    public const string Treasurer = "Treasurer";
    public const string Webmaster = "Webmaster";
    public const string Executive = "Executive";
    public const string Head = "Head";
    public const string ViceHead = "Vice Head";
    public const string Instructor = "Instructor";
    public const string SeniorMember = "Senior Member";
    public const string Member = "Member";
    public const string Participant = "Participant";

    private static readonly FrozenSet<string> AdministrativeRoles = new[]
    {
        Admin,
        OrgAdmin,
        President,
        VicePresident,
        Chair,
        ViceChair,
        Secretary,
        Treasurer,
        Webmaster,
        Executive
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    // Array form exported so EF Core LINQ predicates translate `.Contains(...)` to SQL `IN (...)`.
    public static readonly string[] AdministrativeRoleNames = [.. AdministrativeRoles];

    private static readonly FrozenSet<string> CommitteeLeaderExactRoles = new[]
    {
        Head,
        ViceHead
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsAdminRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && AdministrativeRoles.Contains(role.Trim());

    public static bool IsCommitteeLeaderRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role) || IsAdminRole(role))
            return false;

        var trimmed = role.Trim();
        return CommitteeLeaderExactRoles.Contains(trimmed)
            || trimmed.StartsWith("Head of ", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("Vice Head of ", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("Head", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("Lead", StringComparison.OrdinalIgnoreCase);
    }

    public static OrganizationHierarchyTier GetHierarchyTier(string? role)
    {
        if (IsAdminRole(role))
            return OrganizationHierarchyTier.HighBoard;
        if (IsCommitteeLeaderRole(role))
            return OrganizationHierarchyTier.CommitteeLeadership;
        if (!string.IsNullOrWhiteSpace(role)
            && string.Equals(role.Trim(), Participant, StringComparison.OrdinalIgnoreCase))
            return OrganizationHierarchyTier.Participant;

        return OrganizationHierarchyTier.OrganizingCrew;
    }
}
