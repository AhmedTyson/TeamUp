// Date: 2026-11-21
// Purpose: University catalog service with IMemoryCache read-through caching, automatic invalidation, and audited soft-delete.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Services;

public sealed class UniversityService(
    IAppDbContext db,
    IMemoryCache cache,
    IAuditLogService auditLog) : IUniversityService
{
    public async Task<PagedResult<UniversityDto>> GetUniversitiesAsync(string? search, PageRequest page, CancellationToken ct = default)
    {
        var query = db.Universities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => u.Name.Contains(term) || u.City.Contains(term) || u.Country.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip(page.Skip)
            .Take(page.SafePageSize)
            .Select(u => new UniversityDto(
                u.UniversityId,
                u.Name,
                u.City,
                u.Country,
                u.Website,
                u.Faculties.Count))
            .ToListAsync(ct);

        return new PagedResult<UniversityDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<IReadOnlyList<UniversityDetailDto>> GetAcademicCatalogAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CatalogCacheKeys.AcademicCatalog, out IReadOnlyList<UniversityDetailDto>? cached)
            && cached is not null)
        {
            return cached;
        }

        var catalog = await db.Universities
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UniversityDetailDto(
                u.UniversityId,
                u.Name,
                u.City,
                u.Country,
                u.Website,
                u.Faculties
                    .OrderBy(f => f.Name)
                    .Select(f => new FacultySummaryDto(f.FacultyId, f.Name, f.Code))
                    .ToList()))
            .ToListAsync(ct);

        cache.Set(CatalogCacheKeys.AcademicCatalog, catalog, CatalogCacheKeys.DefaultTtl);
        return catalog;
    }

    public async Task<UniversityDetailDto> GetUniversityByIdAsync(int id, CancellationToken ct = default)
    {
        var uni = await db.Universities
            .AsNoTracking()
            .Include(u => u.Faculties)
            .FirstOrDefaultAsync(u => u.UniversityId == id, ct)
            ?? throw new NotFoundException($"University with ID {id} was not found.");

        var faculties = uni.Faculties
            .OrderBy(f => f.Name)
            .Select(f => new FacultySummaryDto(f.FacultyId, f.Name, f.Code))
            .ToList();

        return new UniversityDetailDto(uni.UniversityId, uni.Name, uni.City, uni.Country, uni.Website, faculties);
    }

    public async Task<UniversityDetailDto> CreateUniversityAsync(CreateUniversityRequest req, CancellationToken ct = default)
    {
        var name = req.Name.Trim();

        if (await db.Universities.AnyAsync(u => u.Name == name, ct))
            throw new ConflictException($"A university named '{name}' already exists.");

        var uni = new University
        {
            Name = name,
            City = req.City.Trim(),
            Country = req.Country.Trim(),
            Website = string.IsNullOrWhiteSpace(req.Website) ? null : req.Website.Trim(),
        };

        db.Universities.Add(uni);
        await db.SaveChangesAsync(ct);
        cache.Remove(CatalogCacheKeys.AcademicCatalog);

        return new UniversityDetailDto(uni.UniversityId, uni.Name, uni.City, uni.Country, uni.Website, []);
    }

    public async Task<UniversityDetailDto> UpdateUniversityAsync(int id, UpdateUniversityRequest req, CancellationToken ct = default)
    {
        var uni = await db.Universities
            .Include(u => u.Faculties)
            .FirstOrDefaultAsync(u => u.UniversityId == id, ct)
            ?? throw new NotFoundException($"University with ID {id} was not found.");

        var name = req.Name.Trim();
        if (!string.Equals(uni.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            if (await db.Universities.AnyAsync(u => u.UniversityId != id && u.Name == name, ct))
                throw new ConflictException($"A university named '{name}' already exists.");
        }

        uni.Name = name;
        uni.City = req.City.Trim();
        uni.Country = req.Country.Trim();
        uni.Website = string.IsNullOrWhiteSpace(req.Website) ? null : req.Website.Trim();

        await db.SaveChangesAsync(ct);
        cache.Remove(CatalogCacheKeys.AcademicCatalog);

        var faculties = uni.Faculties
            .OrderBy(f => f.Name)
            .Select(f => new FacultySummaryDto(f.FacultyId, f.Name, f.Code))
            .ToList();

        return new UniversityDetailDto(uni.UniversityId, uni.Name, uni.City, uni.Country, uni.Website, faculties);
    }

    public async Task DeleteUniversityAsync(int id, CancellationToken ct = default)
    {
        var uni = await db.Universities
            .Include(u => u.Faculties)
            .FirstOrDefaultAsync(u => u.UniversityId == id, ct)
            ?? throw new NotFoundException($"University with ID {id} was not found.");

        var uniName = uni.Name;

        // Cascade soft-delete to faculties
        foreach (var faculty in uni.Faculties)
        {
            db.Faculties.Remove(faculty);
        }

        db.Universities.Remove(uni);
        await db.SaveChangesAsync(ct);
        cache.Remove(CatalogCacheKeys.AcademicCatalog);

        await auditLog.RecordAsync(
            "EntitySoftDeleted",
            "University",
            id.ToString(),
            oldValues: new { id, Name = uniName },
            newValues: new { DeletedAt = uni.DeletedAt },
            ct: ct);
    }
}
