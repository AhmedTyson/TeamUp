using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class AuthService(
    AppDbContext db,
    IJwtTokenService jwt,
    IPasswordHasher<Student> hasher,
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

        var student = new Student
        {
            Email = email,
            PasswordHash = hasher.HashPassword(null!, req.Password),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            AcademicYear = req.AcademicYear,
            FacultyId = req.FacultyId,
            Role = UserRole.Student,
            RefreshToken = jwt.GenerateRefreshToken(),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId, student.Email, student.Role.ToString(), $"{student.FirstName} {student.LastName}");

        var summary = new StudentSummaryDto(
            student.StudentId, student.Email, student.FirstName, student.LastName,
            student.AcademicYear, student.Role.ToString(),
            faculty.FacultyId, faculty.Name, faculty.University.Name);

        return new AuthResponse(accessToken, student.RefreshToken, expiresAt, summary);
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

        student.RefreshToken = jwt.GenerateRefreshToken();
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.Email, student.FirstName, student.LastName,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name);

        return new AuthResponse(accessToken, student.RefreshToken, expiresAt, summary);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new BadRequestException("Refresh token is required.");

        var student = await db.Students
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .FirstOrDefaultAsync(s => s.RefreshToken == req.RefreshToken, ct);

        if (student is null || student.RefreshTokenExpiresAt is null || student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var (accessToken, expiresAt) = jwt.GenerateAccessToken(
            student.StudentId, student.Email, student.Role.ToString(), $"{student.FirstName} {student.LastName}");

        student.RefreshToken = jwt.GenerateRefreshToken();
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await db.SaveChangesAsync(ct);

        var summary = new StudentSummaryDto(
            student.StudentId, student.Email, student.FirstName, student.LastName,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name);

        return new AuthResponse(accessToken, student.RefreshToken, expiresAt, summary);
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
}
