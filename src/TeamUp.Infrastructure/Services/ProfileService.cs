using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Students;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class ProfileService(AppDbContext db)
    : IProfileService, IStudentAccountService, IStudentProfileService, IStudentInterestService
{
    public Task<StudentDetailDto> GetMyAccountAsync(int studentId, CancellationToken ct = default) =>
        GetMyProfileAsync(studentId, ct);

    public Task<StudentDetailDto> UpdateMyAccountAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default) =>
        UpdateMyProfileAsync(studentId, req, ct);

    public async Task<StudentDetailDto> GetMyProfileAsync(int studentId, CancellationToken ct = default)
    {
        var student = await db.Students
            .AsNoTracking()
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .Include(s => s.Profile)
            .Include(s => s.Interests)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        var profileDto = student.Profile is null ? null : new StudentProfileDto(
            student.Profile.Bio, student.Profile.PhotoUrl, student.Profile.GitHubUrl, student.Profile.LinkedInUrl);

        var interests = student.Interests.Select(i => i.Interest).ToList();

        return new StudentDetailDto(
            student.StudentId, student.Email, student.FirstName, student.LastName,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name,
            profileDto, interests);
    }

    public async Task<StudentDetailDto> UpdateMyProfileAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        if (!string.IsNullOrWhiteSpace(req.FirstName))
            student.FirstName = req.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(req.LastName))
            student.LastName = req.LastName.Trim();

        if (req.AcademicYear.HasValue && req.AcademicYear.Value >= 1 && req.AcademicYear.Value <= 7)
            student.AcademicYear = req.AcademicYear.Value;

        await db.SaveChangesAsync(ct);

        return await GetMyProfileAsync(studentId, ct);
    }

    public async Task<StudentProfileDto> GetExtendedProfileAsync(int studentId, CancellationToken ct = default)
    {
        var profile = await db.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.StudentId == studentId, ct);
        if (profile is null)
            return new StudentProfileDto(null, null, null, null);

        return new StudentProfileDto(profile.Bio, profile.PhotoUrl, profile.GitHubUrl, profile.LinkedInUrl);
    }

    public async Task<StudentProfileDto> UpsertExtendedProfileAsync(int studentId, UpsertProfileRequest req, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var profile = await db.StudentProfiles.FirstOrDefaultAsync(p => p.StudentId == studentId, ct);
        if (profile is null)
        {
            profile = new StudentProfile
            {
                StudentId = studentId,
                Bio = req.Bio,
                PhotoUrl = req.PhotoUrl,
                GitHubUrl = req.GitHubUrl,
                LinkedInUrl = req.LinkedInUrl,
            };
            db.StudentProfiles.Add(profile);
        }
        else
        {
            profile.Bio = req.Bio;
            profile.PhotoUrl = req.PhotoUrl;
            profile.GitHubUrl = req.GitHubUrl;
            profile.LinkedInUrl = req.LinkedInUrl;
        }

        await db.SaveChangesAsync(ct);

        return new StudentProfileDto(profile.Bio, profile.PhotoUrl, profile.GitHubUrl, profile.LinkedInUrl);
    }

    public async Task<IReadOnlyList<string>> GetInterestsAsync(int studentId, CancellationToken ct = default)
    {
        return await db.StudentInterests
            .AsNoTracking()
            .Where(i => i.StudentId == studentId)
            .Select(i => i.Interest)
            .ToListAsync(ct);
    }

    public async Task AddInterestAsync(int studentId, string interest, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(interest))
            throw new BadRequestException("Interest tag cannot be empty.");

        var tag = interest.Trim();

        var existing = await db.StudentInterests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.StudentId == studentId && i.Interest == tag, ct);

        if (existing is not null)
        {
            if (existing.DeletedAt is not null)
            {
                existing.DeletedAt = null; // Restore soft-deleted interest
                await db.SaveChangesAsync(ct);
            }
            return;
        }

        db.StudentInterests.Add(new StudentInterest { StudentId = studentId, Interest = tag });
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveInterestAsync(int studentId, string interest, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(interest)) return;

        var tag = interest.Trim();
        var item = await db.StudentInterests.FirstOrDefaultAsync(i => i.StudentId == studentId && i.Interest == tag, ct);
        if (item is not null)
        {
            db.StudentInterests.Remove(item); // Soft delete via interceptor
            await db.SaveChangesAsync(ct);
        }
    }
}
