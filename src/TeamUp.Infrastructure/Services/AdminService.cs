using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class AdminService(AppDbContext db) : IAdminService
{
    public async Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var activeStudents = await db.Students.CountAsync(ct);
        var activeUnis = await db.Universities.CountAsync(ct);
        var activeFaculties = await db.Faculties.CountAsync(ct);

        var deletedStudents = await db.Students.IgnoreQueryFilters().CountAsync(s => s.DeletedAt != null, ct);
        var deletedUnis = await db.Universities.IgnoreQueryFilters().CountAsync(u => u.DeletedAt != null, ct);
        var deletedFaculties = await db.Faculties.IgnoreQueryFilters().CountAsync(f => f.DeletedAt != null, ct);

        var roles = await db.Students
            .GroupBy(s => s.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, ct);

        return new AdminDashboardDto(
            activeStudents,
            activeUnis,
            activeFaculties,
            deletedStudents,
            deletedUnis,
            deletedFaculties,
            roles);
    }

    public async Task<StudentSummaryDto> UpdateStudentRoleAsync(int studentId, UserRole newRole, CancellationToken ct = default)
    {
        var student = await db.Students
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        student.Role = newRole;
        await db.SaveChangesAsync(ct);

        return new StudentSummaryDto(
            student.StudentId,
            student.FirstName,
            student.LastName,
            student.Email,
            student.AcademicYear,
            student.Role.ToString(),
            student.Faculty.FacultyId,
            student.Faculty.Name,
            student.Faculty.University.Name);
    }

    public async Task<IReadOnlyList<SoftDeletedItemDto>> GetSoftDeletedRecordsAsync(string? entityType, CancellationToken ct = default)
    {
        var list = new List<SoftDeletedItemDto>();
        var type = entityType?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(type) || type is "student" or "students")
        {
            var students = await db.Students
                .IgnoreQueryFilters()
                .Where(s => s.DeletedAt != null)
                .OrderByDescending(s => s.DeletedAt)
                .Select(s => new SoftDeletedItemDto(s.StudentId, "Student", $"{s.FirstName} {s.LastName} ({s.Email})", s.DeletedAt!.Value))
                .ToListAsync(ct);
            list.AddRange(students);
        }

        if (string.IsNullOrEmpty(type) || type is "university" or "universities")
        {
            var unis = await db.Universities
                .IgnoreQueryFilters()
                .Where(u => u.DeletedAt != null)
                .OrderByDescending(u => u.DeletedAt)
                .Select(u => new SoftDeletedItemDto(u.UniversityId, "University", u.Name, u.DeletedAt!.Value))
                .ToListAsync(ct);
            list.AddRange(unis);
        }

        if (string.IsNullOrEmpty(type) || type is "faculty" or "faculties")
        {
            var faculties = await db.Faculties
                .IgnoreQueryFilters()
                .Where(f => f.DeletedAt != null)
                .OrderByDescending(f => f.DeletedAt)
                .Select(f => new SoftDeletedItemDto(f.FacultyId, "Faculty", $"{f.Name} ({f.Code})", f.DeletedAt!.Value))
                .ToListAsync(ct);
            list.AddRange(faculties);
        }

        return list.OrderByDescending(x => x.DeletedAt).ToList();
    }

    public async Task RestoreEntityAsync(string entityType, int id, CancellationToken ct = default)
    {
        var type = entityType.Trim().ToLowerInvariant();

        switch (type)
        {
            case "student" or "students":
                var student = await db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StudentId == id, ct)
                    ?? throw new NotFoundException($"Student with ID {id} was not found.");
                if (student.DeletedAt is not null)
                {
                    student.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                }
                break;

            case "university" or "universities":
                var uni = await db.Universities.IgnoreQueryFilters()
                    .Include(u => u.Faculties)
                    .FirstOrDefaultAsync(u => u.UniversityId == id, ct)
                    ?? throw new NotFoundException($"University with ID {id} was not found.");
                if (uni.DeletedAt is not null)
                {
                    uni.DeletedAt = null;
                    foreach (var f in uni.Faculties)
                    {
                        f.DeletedAt = null;
                    }
                    await db.SaveChangesAsync(ct);
                }
                break;

            case "faculty" or "faculties":
                var faculty = await db.Faculties.IgnoreQueryFilters()
                    .Include(f => f.University)
                    .FirstOrDefaultAsync(f => f.FacultyId == id, ct)
                    ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

                if (faculty.University.DeletedAt is not null)
                    throw new ConflictException(
                        $"Cannot restore faculty '{faculty.Name}' because its parent university '{faculty.University.Name}' is soft-deleted. Restore the university first.");

                if (faculty.DeletedAt is not null)
                {
                    faculty.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                }
                break;

            default:
                throw new BadRequestException($"Unsupported entity type '{entityType}'. Supported types: student, university, faculty.");
        }
    }
}
