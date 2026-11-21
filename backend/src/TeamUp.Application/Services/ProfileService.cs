// Date: 2026-11-21
// Purpose: Student account, extended profile, privacy-filtered directory, and audited soft-delete service (STRIDE THR-I-01, THR-R-01, THR-E-02).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Students;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class ProfileService(
    IAppDbContext db,
    ISecurityStampValidator stampValidator,
    IAuditLogService auditLog)
    : IStudentAccountService, IStudentProfileService, IStudentInterestService
{
    public async Task<StudentDetailDto> GetMyAccountAsync(int studentId, CancellationToken ct = default)
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
            student.StudentId, student.FirstName, student.LastName, student.Email,
            student.AcademicYear, student.Role.ToString(),
            student.Faculty.FacultyId, student.Faculty.Name, student.Faculty.University.Name,
            profileDto, interests, student.ShowEmailInDirectory);
    }

    public async Task<StudentDetailDto> UpdateMyAccountAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        if (!string.IsNullOrWhiteSpace(req.FirstName))
            student.FirstName = req.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(req.LastName))
            student.LastName = req.LastName.Trim();

        if (req.AcademicYear.HasValue && req.AcademicYear.Value >= 1 && req.AcademicYear.Value <= 7)
            student.AcademicYear = req.AcademicYear.Value;

        if (req.ShowEmailInDirectory.HasValue)
            student.ShowEmailInDirectory = req.ShowEmailInDirectory.Value;

        await db.SaveChangesAsync(ct);

        return await GetMyAccountAsync(studentId, ct);
    }

    public async Task<PagedResult<StudentCardDto>> GetStudentsDirectoryAsync(
        StudentDirectoryQuery query,
        int viewerStudentId = 0,
        string? viewerRole = null,
        CancellationToken ct = default)
    {
        var page = query.ToPageRequest();
        var isAdmin = string.Equals(viewerRole, nameof(UserRole.PlatformAdmin), StringComparison.OrdinalIgnoreCase);
        var dbQuery = db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            dbQuery = dbQuery.Where(s =>
                s.FirstName.Contains(term)
                || s.LastName.Contains(term)
                || ((isAdmin || s.StudentId == viewerStudentId || s.ShowEmailInDirectory) && s.Email.Contains(term)));
        }

        if (query.FacultyId.HasValue)
            dbQuery = dbQuery.Where(s => s.FacultyId == query.FacultyId.Value);

        if (query.AcademicYear.HasValue)
            dbQuery = dbQuery.Where(s => s.AcademicYear == query.AcademicYear.Value);

        var total = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Skip(page.Skip)
            .Take(page.SafePageSize)
            .Select(s => new StudentCardDto(
                s.StudentId,
                s.FirstName,
                s.LastName,
                isAdmin || s.StudentId == viewerStudentId || s.ShowEmailInDirectory ? s.Email : null,
                s.AcademicYear,
                s.Role.ToString(),
                s.Faculty.FacultyId,
                s.Faculty.Name,
                s.Faculty.University.Name,
                s.Interests.Select(i => i.Interest).ToList()))
            .ToListAsync(ct);

        return new PagedResult<StudentCardDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<StudentPublicProfileDto> GetStudentPublicProfileByIdAsync(int studentId, CancellationToken ct = default)
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

        return new StudentPublicProfileDto(
            student.StudentId,
            student.FirstName,
            student.LastName,
            student.AcademicYear,
            student.Faculty.Name,
            student.Faculty.University.Name,
            profileDto,
            interests);
    }

    public async Task DeleteStudentAccountAsync(int studentId, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        var snapshot = new { student.StudentId, student.Email, Role = student.Role.ToString() };

        student.RefreshToken = null;
        student.RefreshTokenExpiresAt = null;
        student.SecurityStamp = Guid.NewGuid();

        // Clean task assignments for soft-deleted student
        var assignedTasks = await db.ProjectTasks.Where(t => t.AssignedStudentId == studentId).ToListAsync(ct);
        foreach (var task in assignedTasks)
        {
            task.AssignedStudentId = null;
        }

        // Persist security stamp rotation & task unassignment before soft-delete interceptor freezes non-DeletedAt props
        await db.SaveChangesAsync(ct);

        db.Students.Remove(student);
        await db.SaveChangesAsync(ct);
        stampValidator.Invalidate(studentId);

        await auditLog.RecordAsync(
            "EntitySoftDeleted",
            "Student",
            studentId.ToString(),
            oldValues: snapshot,
            newValues: new { DeletedAt = student.DeletedAt },
            ct: ct);
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
                existing.DeletedAt = null;
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
            db.StudentInterests.Remove(item);
            await db.SaveChangesAsync(ct);
        }
    }
}
