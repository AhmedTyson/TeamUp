// Date: 2026-11-21
// Purpose: Faculty catalog service with academic catalog cache invalidation and audited soft-delete.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Services;

public sealed class FacultyService(
    IAppDbContext db,
    HybridCache cache,
    IAuditLogService auditLog) : IFacultyService
{
    #region Queries

    public async Task<IReadOnlyList<FacultyDto>> GetFacultiesByUniversityIdAsync(int universityId, CancellationToken ct = default)
    {
        var uniExists = await db.Universities.AnyAsync(u => u.UniversityId == universityId, ct);
        if (!uniExists)
            throw new NotFoundException($"University with ID {universityId} was not found.");

        return await db.Faculties
            .AsNoTracking()
            .Where(f => f.UniversityId == universityId)
            .OrderBy(f => f.Name)
            .Select(f => new FacultyDto(
                f.FacultyId,
                f.Name,
                f.Code,
                f.UniversityId,
                f.University.Name,
                f.Students.Count))
            .ToListAsync(ct);
    }

    public async Task<FacultyDto> GetFacultyByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await db.Faculties
            .AsNoTracking()
            .Where(f => f.FacultyId == id)
            .Select(f => new FacultyDto(
                f.FacultyId,
                f.Name,
                f.Code,
                f.UniversityId,
                f.University.Name,
                f.Students.Count))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

        return dto;
    }

    #endregion

    #region Mutations

    public async Task<FacultyDto> CreateFacultyAsync(int universityId, CreateFacultyRequest req, CancellationToken ct = default)
    {
        var university = await db.Universities.FirstOrDefaultAsync(u => u.UniversityId == universityId, ct)
            ?? throw new NotFoundException($"University with ID {universityId} was not found.");

        var code = req.Code.Trim().ToUpperInvariant();
        var name = req.Name.Trim();

        if (await db.Faculties.AnyAsync(f => f.UniversityId == universityId && f.Code == code, ct))
            throw new ConflictException($"A faculty with code '{code}' already exists in {university.Name}.");

        var faculty = new Faculty
        {
            Name = name,
            Code = code,
            UniversityId = universityId,
        };

        db.Faculties.Add(faculty);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CatalogCacheKeys.AcademicCatalog, ct);

        return new FacultyDto(
            faculty.FacultyId,
            faculty.Name,
            faculty.Code,
            universityId,
            university.Name,
            0);
    }

    public async Task<FacultyDto> UpdateFacultyAsync(int id, UpdateFacultyRequest req, CancellationToken ct = default)
    {
        var faculty = await db.Faculties
            .Include(f => f.University)
            .FirstOrDefaultAsync(f => f.FacultyId == id, ct)
            ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

        var code = req.Code.Trim().ToUpperInvariant();
        var name = req.Name.Trim();

        if (!string.Equals(faculty.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            if (await db.Faculties.AnyAsync(f => f.UniversityId == faculty.UniversityId && f.FacultyId != id && f.Code == code, ct))
                throw new ConflictException($"A faculty with code '{code}' already exists in {faculty.University.Name}.");
        }

        faculty.Name = name;
        faculty.Code = code;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CatalogCacheKeys.AcademicCatalog, ct);

        var studentsCount = await db.Students.AsNoTracking().CountAsync(s => s.FacultyId == faculty.FacultyId, ct);

        return new FacultyDto(
            faculty.FacultyId,
            faculty.Name,
            faculty.Code,
            faculty.UniversityId,
            faculty.University.Name,
            studentsCount);
    }

    public async Task DeleteFacultyAsync(int id, CancellationToken ct = default)
    {
        var faculty = await db.Faculties.FirstOrDefaultAsync(f => f.FacultyId == id, ct)
            ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

        var code = faculty.Code;
        var name = faculty.Name;
        db.Faculties.Remove(faculty);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CatalogCacheKeys.AcademicCatalog, ct);

        await auditLog.RecordAsync(
            "EntitySoftDeleted",
            "Faculty",
            id.ToString(),
            oldValues: new { id, Name = name, Code = code },
            newValues: new { DeletedAt = faculty.DeletedAt },
            ct: ct);
    }

    #endregion
}
