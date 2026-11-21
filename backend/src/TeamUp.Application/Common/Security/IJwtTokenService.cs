// Date: 2026-11-21
// Purpose: JWT signing contract and custom claim constants supporting SecurityStamp revocation and scoped org_admin claims.

namespace TeamUp.Application.Common.Security;

public static class TeamUpClaimTypes
{
    public const string SecurityStamp = "security_stamp";
    public const string OrgAdmin = "org_admin";
}

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        int studentId,
        string email,
        string role,
        string fullName,
        Guid securityStamp = default,
        IEnumerable<int>? adminOrganizationIds = null);

    string GenerateRefreshToken();
    string HashToken(string token);
}
