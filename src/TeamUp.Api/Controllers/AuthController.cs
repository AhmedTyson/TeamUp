using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Controllers;

[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return CreatedResponse(result, "Student registered successfully.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return OkResponse(result, "Login successful.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request, ct);
        return OkResponse(result, "Token refreshed successfully.");
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await authService.LogoutAsync(CurrentStudentId, ct);
        return NoContentResponse();
    }
}
