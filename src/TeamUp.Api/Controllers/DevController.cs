using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/dev")]
[AllowAnonymous]
public class DevController : ControllerBase
{
    // Helper endpoint to generate ready-to-use JWT Bearer test tokens
    [HttpGet("test-tokens")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTestTokens()
    {
        // 1. Student Token (ID = 1)
        var studentToken = JwtTokenService.GenerateToken("1", "student1@teamup.edu", "Student");

        // 2. OrgAdmin Token (ID = orgadmin-1 / std-ibrahim, OrgAdmin for org-ieee)
        var orgAdminToken = JwtTokenService.GenerateToken("orgadmin-1", "orgadmin1@teamup.edu", "Student");

        // 3. PlatformAdmin Token (ID = platformadmin-1 / admin-master)
        var platformAdminToken = JwtTokenService.GenerateToken("platformadmin-1", "platformadmin1@teamup.edu", "PlatformAdmin");

        // Also include standard seeded IDs for convenience
        var sarahToken = JwtTokenService.GenerateToken("std-sarah", "sarah@teamup.edu", "Student");
        var ibrahimToken = JwtTokenService.GenerateToken("std-ibrahim", "ibrahim@teamup.edu", "Student");
        var masterAdminToken = JwtTokenService.GenerateToken("admin-master", "admin@teamup.edu", "PlatformAdmin");

        return Ok(new
        {
            Message = "Valid JWT Bearer tokens generated using configured SecretKey, Issuer, and Audience.",
            Usage = "Copy any token below and paste it into Swagger's Authorize dialog as: Bearer <token>",
            Tokens = new
            {
                Student = new
                {
                    StudentId = "1",
                    Email = "student1@teamup.edu",
                    Role = "Student",
                    Token = studentToken
                },
                OrgAdmin = new
                {
                    StudentId = "orgadmin-1",
                    OrganizationId = "org-ieee",
                    Email = "orgadmin1@teamup.edu",
                    Role = "OrgAdmin (OrganizationMember)",
                    Token = orgAdminToken
                },
                PlatformAdmin = new
                {
                    StudentId = "platformadmin-1",
                    Email = "platformadmin1@teamup.edu",
                    Role = "PlatformAdmin",
                    Token = platformAdminToken
                }
            },
            AlternativeSeededTokens = new
            {
                Student_Sarah = sarahToken,
                OrgAdmin_Ibrahim = ibrahimToken,
                PlatformAdmin_Master = masterAdminToken
            }
        });
    }
}

