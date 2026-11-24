// Date: 2026-11-21
// Purpose: Authentication service with SecurityStamp revocation, 15-minute reset expiry, and tenant-scoped org_admin JWT claims (STRIDE THR-S-03, THR-E-01, THR-E-02).

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class AuthService(
    IAppDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher<Student> hasher,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions,
    IOrganizationAuthorizationService orgAuth,
    ISecurityStampValidator stampValidator,
    ILogger<AuthService> logger) : IAuthService
{
    /// <summary>Failures allowed before escalating per-account delays start.</summary>
    public const int FreeFailedLoginAttempts = 5;
    public static readonly TimeSpan MaxLockout = TimeSpan.FromMinutes(15);
    /// <summary>A just-rotated token presented again within this window is treated as a client race, not theft.</summary>
    public static readonly TimeSpan RefreshReuseGrace = TimeSpan.FromSeconds(10);

    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            throw new BadRequestException("Email and password are required.");

        var email = req.Email.Trim().ToLowerInvariant();

        if (await db.Students.AnyAsync(s => s.Email == email, ct))
            throw new ConflictException("A student with this email address already exists.");

        var faculty = await db.Faculties
            .AsNoTracking()
            .Include(f => f.University)
            .FirstOrDefaultAsync(f => f.FacultyId == req.FacultyId, ct)
            ?? throw new NotFoundException($"Faculty with ID {req.FacultyId} was not found.");

        var rawRefreshToken = jwt.GenerateRefreshToken();
        var student = new Student
        {
            Email = email,
            PasswordHash = hasher.HashPassword(null!, req.Password),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            AcademicYear = req.AcademicYear,
            FacultyId = req.FacultyId,
            Role = UserRole.Student,
            SecurityStamp = Guid.NewGuid(),
            RefreshToken = jwt.HashToken(rawRefreshToken),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId,
            student.Email,
            student.Role.ToString(),
            $"{student.FirstName} {student.LastName}",
            student.SecurityStamp,
            []);

        var summary = new StudentSummaryDto(
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            faculty.FacultyId, faculty.Name, faculty.University.Name);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt, summary);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            throw new BadRequestException("Email and password are required.");

        var email = req.Email.Trim().ToLowerInvariant();

        var student = await db.Students
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .Include(s => s.Profile)
            .FirstOrDefaultAsync(s => s.Email == email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        // Escalating per-account delay (not a permanent lockout, so it cannot be abused for lasting DoS).
        // The response is identical to a wrong password so lockout state does not reveal the account.
        if (student.LockoutEndUtc is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            logger.LogWarning("Login rejected for StudentId {StudentId}: account in temporary lockout.", student.StudentId);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var result = hasher.VerifyHashedPassword(student, student.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            student.FailedLoginCount++;
            if (student.FailedLoginCount > FreeFailedLoginAttempts)
            {
                var exponent = Math.Min(student.FailedLoginCount - FreeFailedLoginAttempts - 1, 10);
                var delay = TimeSpan.FromSeconds(Math.Min(30 * Math.Pow(2, exponent), MaxLockout.TotalSeconds));
                student.LockoutEndUtc = DateTime.UtcNow.Add(delay);
                logger.LogWarning("StudentId {StudentId} reached {Count} failed logins; locked for {Seconds}s.",
                    student.StudentId, student.FailedLoginCount, (int)delay.TotalSeconds);
            }
            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid email or password.");
        }

        student.FailedLoginCount = 0;
        student.LockoutEndUtc = null;

        if (student.SecurityStamp == Guid.Empty)
            student.SecurityStamp = Guid.NewGuid();

        var adminOrgIds = await orgAuth.GetAdminOrganizationIdsAsync(student.StudentId, ct);
        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId,
            student.Email,
            student.Role.ToString(),
            $"{student.FirstName} {student.LastName}",
            student.SecurityStamp,
            adminOrgIds);

        var rawRefreshToken = jwt.GenerateRefreshToken();
        student.RefreshToken = jwt.HashToken(rawRefreshToken);
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        student.PreviousRefreshToken = null;
        student.PreviousRefreshTokenRotatedAt = null;
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name,
            student.Profile?.PhotoUrl);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt, summary);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new BadRequestException("Refresh token is required.");

        var hashedToken = jwt.HashToken(req.RefreshToken.Trim());

        var student = await db.Students
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .Include(s => s.Profile)
            .FirstOrDefaultAsync(s => s.RefreshToken == hashedToken, ct);

        if (student is null)
        {
            await HandlePossibleRefreshReuseAsync(hashedToken, ct);
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        if (student.RefreshTokenExpiresAt is null || student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        if (student.SecurityStamp == Guid.Empty)
            student.SecurityStamp = Guid.NewGuid();

        var adminOrgIds = await orgAuth.GetAdminOrganizationIdsAsync(student.StudentId, ct);
        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId,
            student.Email,
            student.Role.ToString(),
            $"{student.FirstName} {student.LastName}",
            student.SecurityStamp,
            adminOrgIds);

        var rawRefreshToken = jwt.GenerateRefreshToken();
        student.PreviousRefreshToken = student.RefreshToken;
        student.PreviousRefreshTokenRotatedAt = DateTime.UtcNow;
        student.RefreshToken = jwt.HashToken(rawRefreshToken);
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name,
            student.Profile?.PhotoUrl);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt, summary);
    }

    /// <summary>
    /// A token that is no longer current but equals the previous generation was already rotated. Outside the short
    /// race grace window that means a stale copy is being replayed, so the whole session is revoked (RFC 6819 5.2.2.3).
    /// </summary>
    private async Task HandlePossibleRefreshReuseAsync(string hashedToken, CancellationToken ct)
    {
        var victim = await db.Students.FirstOrDefaultAsync(s => s.PreviousRefreshToken == hashedToken, ct);
        if (victim is null || victim.PreviousRefreshTokenRotatedAt is null)
            return;

        if (DateTime.UtcNow - victim.PreviousRefreshTokenRotatedAt.Value <= RefreshReuseGrace)
            return;

        logger.LogWarning("Refresh token reuse detected for StudentId {StudentId}; session revoked.", victim.StudentId);
        victim.RefreshToken = null;
        victim.RefreshTokenExpiresAt = null;
        victim.PreviousRefreshToken = null;
        victim.PreviousRefreshTokenRotatedAt = null;
        victim.SecurityStamp = Guid.NewGuid();
        await db.SaveChangesAsync(ct);
        stampValidator.Invalidate(victim.StudentId);
    }

    public async Task LogoutAsync(int studentId, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct);
        if (student is not null)
        {
            student.RefreshToken = null;
            student.RefreshTokenExpiresAt = null;
            student.PreviousRefreshToken = null;
            student.PreviousRefreshTokenRotatedAt = null;
            student.SecurityStamp = Guid.NewGuid();
            await db.SaveChangesAsync(ct);
            stampValidator.Invalidate(studentId);
        }
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var student = await db.Students.FirstOrDefaultAsync(s => s.Email == email, ct);

        if (student is not null)
        {
            var rawResetToken = jwt.GenerateRefreshToken();
            student.PasswordResetToken = jwt.HashToken(rawResetToken);
            student.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(Math.Clamp(_jwtOptions.PasswordResetTokenMinutes, 5, 60));
            await db.SaveChangesAsync(ct);

            await emailSender.SendPasswordResetEmailAsync(student.Email, rawResetToken, ct);
        }

        return new ForgotPasswordResponse("If your email is registered, a password reset link has been dispatched.");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var student = await db.Students.FirstOrDefaultAsync(s => s.Email == email, ct)
            ?? throw new UnauthorizedException("Invalid email or reset token.");

        var incomingHash = jwt.HashToken(req.Token.Trim());

        if (string.IsNullOrWhiteSpace(student.PasswordResetToken) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(student.PasswordResetToken),
                Encoding.UTF8.GetBytes(incomingHash)) ||
            student.PasswordResetTokenExpiresAt is null ||
            student.PasswordResetTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired password reset token.");
        }

        student.PasswordHash = hasher.HashPassword(student, req.NewPassword);
        student.PasswordResetToken = null;
        student.PasswordResetTokenExpiresAt = null;

        // Immediately invalidate all active access & refresh tokens on password change (STRIDE THR-E-02)
        student.RefreshToken = null;
        student.RefreshTokenExpiresAt = null;
        student.PreviousRefreshToken = null;
        student.PreviousRefreshTokenRotatedAt = null;
        student.FailedLoginCount = 0;
        student.LockoutEndUtc = null;
        student.SecurityStamp = Guid.NewGuid();

        await db.SaveChangesAsync(ct);
        stampValidator.Invalidate(student.StudentId);
    }
}
