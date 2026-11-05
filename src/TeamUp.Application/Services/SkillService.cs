using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Skills;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Services;

public sealed class SkillService(IAppDbContext db) : ISkillService
{
    public async Task<IReadOnlyList<SkillDto>> GetSkillsAsync(string? category, string? search, CancellationToken ct = default)
    {
        var query = db.Skills.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            query = query.Where(s => s.Category == cat);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Name.Contains(term) || s.Category.Contains(term));
        }

        return await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new SkillDto(s.SkillId, s.Name, s.Category, s.Description))
            .ToListAsync(ct);
    }

    public async Task<SkillDto> GetSkillByIdAsync(int id, CancellationToken ct = default)
    {
        var skill = await db.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.SkillId == id, ct)
            ?? throw new NotFoundException($"Skill with ID {id} was not found.");

        return new SkillDto(skill.SkillId, skill.Name, skill.Category, skill.Description);
    }

    public async Task<SkillDto> CreateSkillAsync(CreateSkillRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        var existing = await db.Skills.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Name == name, ct);

        if (existing is not null)
        {
            if (existing.DeletedAt is null)
                throw new ConflictException($"A skill named '{name}' already exists in the master catalog.");

            // Restore soft-deleted skill
            existing.DeletedAt = null;
            existing.Category = request.Category.Trim();
            existing.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            await db.SaveChangesAsync(ct);
            return new SkillDto(existing.SkillId, existing.Name, existing.Category, existing.Description);
        }

        var skill = new Skill
        {
            Name = name,
            Category = request.Category.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        };

        db.Skills.Add(skill);
        await db.SaveChangesAsync(ct);

        return new SkillDto(skill.SkillId, skill.Name, skill.Category, skill.Description);
    }

    public async Task<SkillDto> UpdateSkillAsync(int id, UpdateSkillRequest request, CancellationToken ct = default)
    {
        var skill = await db.Skills.FirstOrDefaultAsync(s => s.SkillId == id, ct)
            ?? throw new NotFoundException($"Skill with ID {id} was not found.");

        var name = request.Name.Trim();
        if (!string.Equals(skill.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            if (await db.Skills.AnyAsync(s => s.SkillId != id && s.Name == name, ct))
                throw new ConflictException($"A skill named '{name}' already exists in the master catalog.");
        }

        skill.Name = name;
        skill.Category = request.Category.Trim();
        skill.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await db.SaveChangesAsync(ct);

        return new SkillDto(skill.SkillId, skill.Name, skill.Category, skill.Description);
    }

    public async Task DeleteSkillAsync(int id, CancellationToken ct = default)
    {
        var skill = await db.Skills.FirstOrDefaultAsync(s => s.SkillId == id, ct)
            ?? throw new NotFoundException($"Skill with ID {id} was not found.");

        db.Skills.Remove(skill);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StudentSkillDto>> GetStudentSkillsAsync(int studentId, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        return await db.StudentSkills
            .AsNoTracking()
            .Where(ss => ss.StudentId == studentId)
            .Include(ss => ss.Skill)
            .OrderBy(ss => ss.Skill.Category)
            .ThenBy(ss => ss.Skill.Name)
            .Select(ss => new StudentSkillDto(
                ss.SkillId,
                ss.Skill.Name,
                ss.Skill.Category,
                ss.ProficiencyLevel,
                ss.YearsOfExperience))
            .ToListAsync(ct);
    }

    public async Task<StudentSkillDto> ClaimSkillAsync(int studentId, int skillId, ClaimSkillRequest request, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var skill = await db.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId, ct)
            ?? throw new NotFoundException($"Skill with ID {skillId} was not found in the master catalog.");

        var studentSkill = await db.StudentSkills
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SkillId == skillId, ct);

        if (studentSkill is null)
        {
            studentSkill = new StudentSkill
            {
                StudentId = studentId,
                SkillId = skillId,
                ProficiencyLevel = request.ProficiencyLevel,
                YearsOfExperience = request.YearsOfExperience,
            };
            db.StudentSkills.Add(studentSkill);
        }
        else
        {
            studentSkill.DeletedAt = null;
            studentSkill.ProficiencyLevel = request.ProficiencyLevel;
            studentSkill.YearsOfExperience = request.YearsOfExperience;
        }

        await db.SaveChangesAsync(ct);

        return new StudentSkillDto(
            skill.SkillId,
            skill.Name,
            skill.Category,
            studentSkill.ProficiencyLevel,
            studentSkill.YearsOfExperience);
    }

    public async Task RemoveClaimedSkillAsync(int studentId, int skillId, CancellationToken ct = default)
    {
        var studentSkill = await db.StudentSkills
            .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SkillId == skillId, ct);

        if (studentSkill is not null)
        {
            db.StudentSkills.Remove(studentSkill);
            await db.SaveChangesAsync(ct);
        }
    }
}
