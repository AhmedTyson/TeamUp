using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeamUp.Api.Data;

namespace TeamUp.Api.Auth;

public class JwtTokenService
{
    public const string SecretKey = "teamup-super-secret-key-t22-t27-master-spec-csharp-net10";
    public const string Issuer = "TeamUp.Api";
    public const string Audience = "TeamUp.Client";

    public static string GenerateToken(string studentId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, studentId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("sub", studentId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public static class ClaimsPrincipalExtensions
{
    public static string? GetStudentId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? user.FindFirst("sub")?.Value;
    }

    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public static bool IsPlatformAdmin(this ClaimsPrincipal user)
    {
        return user.GetRole() == "PlatformAdmin";
    }

    public static async Task<bool> IsOrgAdminAsync(this ClaimsPrincipal user, TeamUpDbContext db, string organizationId)
    {
        if (user.IsPlatformAdmin()) return true;

        var studentId = user.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return false;

        return await db.OrganizationMembers.AnyAsync(om => 
            om.OrganizationID == organizationId && 
            om.StudentID == studentId && 
            om.Role == "OrgAdmin");
    }
}

