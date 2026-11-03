using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class UniversityService(AppDbContext db) : IUniversityService
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

        // Cascade soft-delete to faculties
        foreach (var faculty in uni.Faculties)
        {
            db.Faculties.Remove(faculty);
        }

        db.Universities.Remove(uni);
        await db.SaveChangesAsync(ct);
    }
}
