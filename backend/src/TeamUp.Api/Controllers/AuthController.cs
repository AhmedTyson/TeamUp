using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Auth;

namespace TeamUp.Api.Controllers;

[Route("api/v1/auth")]
[EnableRateLimiting("auth-policy")]
public sealed class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesCreated<AuthResponse>, ProducesConflict]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return CreatedResponse(result, "Student registered successfully.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesOk<AuthResponse>]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return OkResponse(result, "Login successful.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesOk<AuthResponse>]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request, ct);
        return OkResponse(result, "Token refreshed successfully.");
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesNoContent]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await authService.LogoutAsync(CurrentStudentId, ct);
        return NoContentResponse();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesOk<ForgotPasswordResponse>]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await authService.ForgotPasswordAsync(request, ct);
        return OkResponse(result, "Password reset request processed.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesOk]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await authService.ResetPasswordAsync(request, ct);
        return OkMessage("Password reset successfully. You can now login with your new credentials.");
    }

    [HttpPost("send-verification-email")]
    [Authorize]
    [ProducesOk<SendVerificationEmailResponse>]
    public async Task<IActionResult> SendVerificationEmail(CancellationToken ct)
    {
        var result = await authService.SendVerificationEmailAsync(CurrentStudentId, ct);
        return OkResponse(result, result.Message);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesOk]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        await authService.VerifyEmailAsync(request, ct);
        return OkMessage("Email verified successfully. Your Verified Student badge is now active.");
    }
}
