// Date: 2026-11-21
// Purpose: Platform admin service with guarded role escalation, immediate SecurityStamp revocation, universal restore, and immutable audit logs (STRIDE THR-R-01, THR-E-02, THR-E-03).

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class AdminService(
    IAppDbContext db,
    IOptions<AdminOptions> adminOptions,
    ISecurityStampValidator stampValidator,
    IAuditLogService auditLog,
    IMemoryCache cache) : IAdminService
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

    public async Task<StudentSummaryDto> UpdateStudentRoleAsync(
        int studentId, UserRole newRole, int actorStudentId = 0, CancellationToken ct = default)
    {
        var student = await db.Students
            .Include(s => s.Faculty)
            .ThenInclude(f => f.University)
            .Include(s => s.Profile)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        var oldRole = student.Role;

        // STRIDE THR-E-03: protect PlatformAdmin elevation from single-admin runtime escalation
        if (newRole == UserRole.PlatformAdmin && oldRole != UserRole.PlatformAdmin
            && !adminOptions.Value.AllowRuntimePlatformAdminPromotion)
        {
            throw new ForbiddenException(
                "Runtime elevation to PlatformAdmin is restricted. Provision PlatformAdmin accounts via bootstrap configuration or database seeders.");
        }

        if (actorStudentId > 0 && actorStudentId == studentId
            && oldRole == UserRole.PlatformAdmin && newRole != UserRole.PlatformAdmin)
        {
            throw new ConflictException("Administrators cannot demote their own PlatformAdmin account.");
        }

        if (oldRole == UserRole.PlatformAdmin && newRole != UserRole.PlatformAdmin)
        {
            var platformAdminCount = await db.Students.CountAsync(s => s.Role == UserRole.PlatformAdmin, ct);
            if (platformAdminCount <= 1)
                throw new ConflictException("Cannot demote the last active PlatformAdmin account.");
        }

        student.Role = newRole;
        // STRIDE THR-E-02: immediately invalidate existing access and refresh tokens on role modification
        student.SecurityStamp = Guid.NewGuid();
        student.RefreshToken = null;
        student.RefreshTokenExpiresAt = null;
        await db.SaveChangesAsync(ct);

        stampValidator.Invalidate(studentId);

        await auditLog.RecordAsync(
            "RoleChanged",
            "Student",
            studentId.ToString(),
            oldValues: new { Role = oldRole.ToString() },
            newValues: new { Role = newRole.ToString() },
            actorStudentIdOverride: actorStudentId > 0 ? actorStudentId : null,
            ct: ct);

        return new StudentSummaryDto(
            student.StudentId,
            student.FirstName,
            student.LastName,
            student.Email,
            student.AcademicYear,
            student.Role.ToString(),
            student.Faculty.FacultyId,
            student.Faculty.Name,
            student.Faculty.University.Name,
            student.Profile?.PhotoUrl);
    }

    public async Task<IReadOnlyList<SoftDeletedItemDto>> GetSoftDeletedRecordsAsync(string? entityType, CancellationToken ct = default)
    {
        var list = new List<SoftDeletedItemDto>();
        var type = entityType?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(type) || type is "student" or "students")
        {
            list.AddRange(await db.Students.AsNoTracking().IgnoreQueryFilters()
                .Where(s => s.DeletedAt != null)
                .Select(s => new SoftDeletedItemDto(s.StudentId, "Student", $"{s.FirstName} {s.LastName} ({s.Email})", s.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "university" or "universities")
        {
            list.AddRange(await db.Universities.AsNoTracking().IgnoreQueryFilters()
                .Where(u => u.DeletedAt != null)
                .Select(u => new SoftDeletedItemDto(u.UniversityId, "University", u.Name, u.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "faculty" or "faculties")
        {
            list.AddRange(await db.Faculties.AsNoTracking().IgnoreQueryFilters()
                .Where(f => f.DeletedAt != null)
                .Select(f => new SoftDeletedItemDto(f.FacultyId, "Faculty", $"{f.Name} ({f.Code})", f.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "skill" or "skills")
        {
            list.AddRange(await db.Skills.AsNoTracking().IgnoreQueryFilters()
                .Where(s => s.DeletedAt != null)
                .Select(s => new SoftDeletedItemDto(s.SkillId, "Skill", $"{s.Name} ({s.Category})", s.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "project" or "projects")
        {
            list.AddRange(await db.Projects.AsNoTracking().IgnoreQueryFilters()
                .Where(p => p.DeletedAt != null)
                .Select(p => new SoftDeletedItemDto(p.ProjectId, "Project", p.Title, p.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "organization" or "organizations")
        {
            list.AddRange(await db.Organizations.AsNoTracking().IgnoreQueryFilters()
                .Where(o => o.DeletedAt != null)
                .Select(o => new SoftDeletedItemDto(o.OrganizationId, "Organization", o.Name, o.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "event" or "events")
        {
            list.AddRange(await db.Events.AsNoTracking().IgnoreQueryFilters()
                .Where(e => e.DeletedAt != null)
                .Select(e => new SoftDeletedItemDto(e.EventId, "Event", e.Title, e.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        if (string.IsNullOrEmpty(type) || type is "badge" or "badges")
        {
            list.AddRange(await db.Badges.AsNoTracking().IgnoreQueryFilters()
                .Where(b => b.DeletedAt != null)
                .Select(b => new SoftDeletedItemDto(b.BadgeId, "Badge", b.Name, b.DeletedAt!.Value))
                .ToListAsync(ct));
        }

        return list.OrderByDescending(x => x.DeletedAt).ToList();
    }

    public async Task RestoreEntityAsync(string entityType, int id, CancellationToken ct = default)
    {
        var type = entityType.Trim().ToLowerInvariant();
        string canonicalType;
        DateTime? previousDeletedAt;

        switch (type)
        {
            case "student" or "students":
                var student = await db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StudentId == id, ct)
                    ?? throw new NotFoundException($"Student with ID {id} was not found.");
                canonicalType = "Student";
                previousDeletedAt = student.DeletedAt;
                if (student.DeletedAt is not null)
                {
                    student.DeletedAt = null;
                    student.SecurityStamp = Guid.NewGuid();
                    await db.SaveChangesAsync(ct);
                    stampValidator.Invalidate(id);
                }
                break;

            case "university" or "universities":
                var uni = await db.Universities.IgnoreQueryFilters()
                    .Include(u => u.Faculties)
                    .FirstOrDefaultAsync(u => u.UniversityId == id, ct)
                    ?? throw new NotFoundException($"University with ID {id} was not found.");
                canonicalType = "University";
                previousDeletedAt = uni.DeletedAt;
                if (uni.DeletedAt is not null)
                {
                    uni.DeletedAt = null;
                    foreach (var f in uni.Faculties)
                        f.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                    cache.Remove(CatalogCacheKeys.AcademicCatalog);
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

                canonicalType = "Faculty";
                previousDeletedAt = faculty.DeletedAt;
                if (faculty.DeletedAt is not null)
                {
                    faculty.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                    cache.Remove(CatalogCacheKeys.AcademicCatalog);
                }
                break;

            case "skill" or "skills":
                var skill = await db.Skills.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SkillId == id, ct)
                    ?? throw new NotFoundException($"Skill with ID {id} was not found.");
                canonicalType = "Skill";
                previousDeletedAt = skill.DeletedAt;
                if (skill.DeletedAt is not null)
                {
                    skill.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                    cache.Remove(CatalogCacheKeys.MasterSkills);
                }
                break;

            case "project" or "projects":
                var project = await db.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProjectId == id, ct)
                    ?? throw new NotFoundException($"Project with ID {id} was not found.");
                canonicalType = "Project";
                previousDeletedAt = project.DeletedAt;
                if (project.DeletedAt is not null)
                {
                    project.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                }
                break;

            case "organization" or "organizations":
                var org = await db.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.OrganizationId == id, ct)
                    ?? throw new NotFoundException($"Organization with ID {id} was not found.");
                canonicalType = "Organization";
                previousDeletedAt = org.DeletedAt;
                if (org.DeletedAt is not null)
                {
                    org.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                }
                break;

            case "event" or "events":
                var evt = await db.Events.IgnoreQueryFilters()
                    .Include(e => e.Organization)
                    .Include(e => e.Sessions)
                    .FirstOrDefaultAsync(e => e.EventId == id, ct)
                    ?? throw new NotFoundException($"Event with ID {id} was not found.");
                if (evt.Organization.DeletedAt is not null)
                    throw new ConflictException(
                        $"Cannot restore event '{evt.Title}' because its parent organization '{evt.Organization.Name}' is soft-deleted. Restore the organization first.");
                canonicalType = "Event";
                previousDeletedAt = evt.DeletedAt;
                if (evt.DeletedAt is not null)
                {
                    evt.DeletedAt = null;
                    foreach (var s in evt.Sessions)
                        s.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                }
                break;

            case "badge" or "badges":
                var badge = await db.Badges.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.BadgeId == id, ct)
                    ?? throw new NotFoundException($"Badge with ID {id} was not found.");
                canonicalType = "Badge";
                previousDeletedAt = badge.DeletedAt;
                if (badge.DeletedAt is not null)
                {
                    badge.DeletedAt = null;
                    await db.SaveChangesAsync(ct);
                    cache.Remove(CatalogCacheKeys.BadgeCatalog);
                }
                break;

            default:
                throw new BadRequestException(
                    $"Unsupported entity type '{entityType}'. Supported types: student, university, faculty, skill, project, organization, event, badge.");
        }

        if (previousDeletedAt is not null)
        {
            await auditLog.RecordAsync(
                "EntityRestored",
                canonicalType,
                id.ToString(),
                oldValues: new { DeletedAt = previousDeletedAt },
                newValues: new { DeletedAt = (DateTime?)null },
                ct: ct);
        }
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? action,
        string? entityType,
        int? actorStudentId,
        PageRequest page,
        CancellationToken ct = default)
    {
        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var act = action.Trim();
            query = query.Where(a => a.Action == act);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var type = entityType.Trim();
            query = query.Where(a => a.EntityType == type);
        }

        if (actorStudentId.HasValue)
            query = query.Where(a => a.ActorStudentId == actorStudentId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.OccurredAt)
            .ThenByDescending(a => a.AuditLogId)
            .Skip(page.Skip)
            .Take(page.SafePageSize)
            .Select(a => new AuditLogDto(
                a.AuditLogId,
                a.ActorStudentId,
                a.ActorEmail,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.ClientIp,
                a.OccurredAt))
            .ToListAsync(ct);

        return new PagedResult<AuditLogDto>(items, page.SafePage, page.SafePageSize, total);
    }
}
