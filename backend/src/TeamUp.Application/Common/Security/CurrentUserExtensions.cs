// Date: 2026-11-21
// Purpose: ClaimsPrincipal extraction helpers for identity, role, SecurityStamp, and scoped organization admin claims.

using System.Security.Claims;

namespace TeamUp.Application.Common.Security;

public static class CurrentUserExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public int StudentId
        {
            get
            {
                var sub = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? principal?.FindFirst("sub")?.Value;
                return int.TryParse(sub, out var id) ? id : 0;
            }
        }

        public string? UserRole =>
            principal?.FindFirst(ClaimTypes.Role)?.Value ?? principal?.FindFirst("role")?.Value;

        public string? Email =>
            principal?.FindFirst(ClaimTypes.Email)?.Value ?? principal?.FindFirst("email")?.Value;

        public Guid? SecurityStamp
        {
            get
            {
                var value = principal?.FindFirst(TeamUpClaimTypes.SecurityStamp)?.Value;
                return Guid.TryParse(value, out var stamp) ? stamp : null;
            }
        }

        public IReadOnlyList<int> AdminOrganizationIds =>
            principal is null
                ? []
                : principal.FindAll(TeamUpClaimTypes.OrgAdmin)
                    .Select(c => int.TryParse(c.Value, out var id) ? id : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

        public int GetStudentId() => principal.StudentId;
        public string? GetUserRole() => principal.UserRole;
        public string? GetEmail() => principal.Email;
        public Guid? GetSecurityStamp() => principal.SecurityStamp;
        public IReadOnlyList<int> GetAdminOrganizationIds() => principal.AdminOrganizationIds;
    }
}
