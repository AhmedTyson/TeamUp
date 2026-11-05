using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Auth;
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
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
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
            RefreshToken = jwt.HashToken(rawRefreshToken),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId, student.Email, student.Role.ToString(), $"{student.FirstName} {student.LastName}");

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
            .FirstOrDefaultAsync(s => s.Email == email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        var result = hasher.VerifyHashedPassword(student, student.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid email or password.");

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId, student.Email, student.Role.ToString(), $"{student.FirstName} {student.LastName}");

        var rawRefreshToken = jwt.GenerateRefreshToken();
        student.RefreshToken = jwt.HashToken(rawRefreshToken);
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name);

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
            .FirstOrDefaultAsync(s => s.RefreshToken == hashedToken, ct);

        if (student is null || student.RefreshTokenExpiresAt is null || student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId, student.Email, student.Role.ToString(), $"{student.FirstName} {student.LastName}");

        var rawRefreshToken = jwt.GenerateRefreshToken();
        student.RefreshToken = jwt.HashToken(rawRefreshToken);
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name);

        return new AuthResponse(accessToken, rawRefreshToken, expiresAt, summary);
    }

    public async Task LogoutAsync(int studentId, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct);
        if (student is not null)
        {
            student.RefreshToken = null;
            student.RefreshTokenExpiresAt = null;
            await db.SaveChangesAsync(ct);
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
            student.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
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

        // Invalidate active session tokens on password change
        student.RefreshToken = null;
        student.RefreshTokenExpiresAt = null;

        await db.SaveChangesAsync(ct);
    }
}
