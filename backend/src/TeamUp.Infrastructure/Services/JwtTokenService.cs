// Date: 2026-11-21
// Purpose: JWT signing service issuing short-lived HMAC-SHA256 access tokens with SecurityStamp and scoped org_admin claims (STRIDE THR-E-01, THR-E-02).

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeamUp.Application.Common.Security;

namespace TeamUp.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider? timeProvider = null) : IJwtTokenService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        int studentId,
        string email,
        string role,
        string fullName,
        Guid securityStamp = default,
        IEnumerable<int>? adminOrganizationIds = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = _time.GetUtcNow().UtcDateTime.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, studentId.ToString()),
            new(ClaimTypes.NameIdentifier, studentId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (securityStamp != Guid.Empty)
        {
            claims.Add(new Claim(TeamUpClaimTypes.SecurityStamp, securityStamp.ToString("D")));
        }

        if (adminOrganizationIds is not null)
        {
            foreach (var orgId in adminOrganizationIds.Distinct())
            {
                if (orgId > 0)
                    claims.Add(new Claim(TeamUpClaimTypes.OrgAdmin, orgId.ToString()));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (TokenHandler.WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
