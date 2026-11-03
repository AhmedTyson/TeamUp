using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Services;

public sealed class FacultyService(AppDbContext db) : IFacultyService
{
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
        var faculty = await db.Faculties
            .AsNoTracking()
            .Include(f => f.University)
            .FirstOrDefaultAsync(f => f.FacultyId == id, ct)
            ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

        return new FacultyDto(
            faculty.FacultyId,
            faculty.Name,
            faculty.Code,
            faculty.UniversityId,
            faculty.University.Name,
            faculty.Students.Count);
    }

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

        return new FacultyDto(
            faculty.FacultyId,
            faculty.Name,
            faculty.Code,
            faculty.UniversityId,
            faculty.University.Name,
            faculty.Students.Count);
    }

    public async Task DeleteFacultyAsync(int id, CancellationToken ct = default)
    {
        var faculty = await db.Faculties.FirstOrDefaultAsync(f => f.FacultyId == id, ct)
            ?? throw new NotFoundException($"Faculty with ID {id} was not found.");

        db.Faculties.Remove(faculty);
        await db.SaveChangesAsync(ct);
    }
}
