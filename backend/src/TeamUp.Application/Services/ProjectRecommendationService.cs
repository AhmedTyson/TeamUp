// Date: 2026-11-24
// Purpose: Deterministic portfolio-to-project recommendation engine ranking recruiting squads by mandatory skills (60%), optional skills (20%), proficiency (10%), and interest overlap (10%).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Projects;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class ProjectRecommendationService(IAppDbContext db) : IProjectRecommendationService
{
    public async Task<IReadOnlyList<ProjectMatchRecommendationDto>> GetRecommendedProjectsForStudentAsync(
        int studentId,
        int maxResults = 5,
        int minMatchPercentage = 1,
        CancellationToken ct = default)
    {
        var safeLimit = Math.Clamp(maxResults, 1, 20);
        var safeMinScore = Math.Clamp(minMatchPercentage, 0, 100);

        var studentSkillsList = await db.StudentSkills
            .AsNoTracking()
            .Where(ss => ss.StudentId == studentId)
            .Select(ss => new { ss.SkillId, ss.Skill.Name, ss.ProficiencyLevel })
            .ToListAsync(ct);

        var studentSkills = studentSkillsList.ToDictionary(
            x => x.SkillId,
            x => (x.Name, x.ProficiencyLevel));

        var studentInterests = await db.StudentInterests
            .AsNoTracking()
            .Where(si => si.StudentId == studentId)
            .Select(si => si.Interest)
            .ToListAsync(ct);

        var candidates = await db.Projects
            .AsNoTracking()
            .Where(p => p.Status == ProjectStatus.Recruiting
                && p.CreatorId != studentId
                && !p.Members.Any(m => m.StudentId == studentId)
                && p.Members.Count() < p.MaxTeamSize
                && !db.ProjectApplications.Any(a =>
                    a.ProjectId == p.ProjectId
                    && a.StudentId == studentId
                    && (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted)))
            .Select(p => new
            {
                p.ProjectId,
                p.Title,
                p.Description,
                p.MaxTeamSize,
                CurrentMemberCount = p.Members.Count(),
                CreatorName = p.Creator.FirstName + " " + p.Creator.LastName,
                UniversityName = p.Creator.Faculty.University.Name,
                FacultyCode = p.Creator.Faculty.Code,
                p.CreatedAt
            })
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return [];

        var candidateIds = candidates.Select(c => c.ProjectId).ToArray();

        var requiredSkillsRows = await db.ProjectRequiredSkills
            .AsNoTracking()
            .Where(prs => candidateIds.Contains(prs.ProjectId))
            .Select(prs => new
            {
                prs.ProjectId,
                prs.SkillId,
                SkillName = prs.Skill.Name,
                prs.RequiredLevel,
                prs.IsMandatory
            })
            .ToListAsync(ct);

        var skillsByProject = requiredSkillsRows
            .GroupBy(r => r.ProjectId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<(int SkillId, string SkillName, ProficiencyLevel RequiredLevel, bool IsMandatory)>)g
                    .Select(x => (x.SkillId, x.SkillName, x.RequiredLevel, x.IsMandatory))
                    .ToList());

        var ranked = new List<(ProjectMatchRecommendationDto Dto, DateTime CreatedAt)>();

        foreach (var project in candidates)
        {
            var reqSkills = skillsByProject.GetValueOrDefault(project.ProjectId)
                ?? Array.Empty<(int SkillId, string SkillName, ProficiencyLevel RequiredLevel, bool IsMandatory)>();

            var (score, matchedSkills, missingSkills, matchedInterests) = ComputeMatchScore(
                studentSkills,
                studentInterests,
                reqSkills,
                project.Title,
                project.Description);

            if (score < safeMinScore)
                continue;

            var openSeats = Math.Max(project.MaxTeamSize - project.CurrentMemberCount, 0);
            ranked.Add((
                new ProjectMatchRecommendationDto(
                    project.ProjectId,
                    project.Title,
                    project.Description,
                    score,
                    project.CurrentMemberCount,
                    project.MaxTeamSize,
                    openSeats,
                    project.CreatorName,
                    project.UniversityName,
                    project.FacultyCode,
                    matchedSkills,
                    missingSkills,
                    matchedInterests),
                project.CreatedAt));
        }

        return ranked
            .OrderByDescending(x => x.Dto.MatchPercentage)
            .ThenByDescending(x => x.Dto.OpenSeats)
            .ThenByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Dto.ProjectId)
            .Take(safeLimit)
            .Select(x => x.Dto)
            .ToList();
    }

    public static (
        int Score,
        IReadOnlyList<string> MatchedSkills,
        IReadOnlyList<string> MissingSkills,
        IReadOnlyList<string> MatchedInterests) ComputeMatchScore(
        IReadOnlyDictionary<int, (string SkillName, ProficiencyLevel Level)> studentSkills,
        IReadOnlyList<string> studentInterests,
        IReadOnlyList<(int SkillId, string SkillName, ProficiencyLevel RequiredLevel, bool IsMandatory)> requiredSkills,
        string projectTitle,
        string projectDescription)
    {
        var mandatory = requiredSkills.Where(r => r.IsMandatory).ToList();
        var optional = requiredSkills.Where(r => !r.IsMandatory).ToList();

        var matchedSkills = new List<string>();
        var missingMandatory = new List<string>();
        var missingOptional = new List<string>();

        var matchedMandatoryCount = 0;
        var matchedOptionalCount = 0;
        var proficiencyMetCount = 0;

        foreach (var req in mandatory)
        {
            if (studentSkills.TryGetValue(req.SkillId, out var owned))
            {
                matchedMandatoryCount++;
                matchedSkills.Add(req.SkillName);
                if ((int)owned.Level >= (int)req.RequiredLevel)
                    proficiencyMetCount++;
            }
            else
            {
                missingMandatory.Add(req.SkillName);
            }
        }

        foreach (var req in optional)
        {
            if (studentSkills.TryGetValue(req.SkillId, out var owned))
            {
                matchedOptionalCount++;
                matchedSkills.Add(req.SkillName);
                if ((int)owned.Level >= (int)req.RequiredLevel)
                    proficiencyMetCount++;
            }
            else
            {
                missingOptional.Add(req.SkillName);
            }
        }

        double total = 0.0;

        if (mandatory.Count > 0)
        {
            // When a project specifies only mandatory skills, allocate the optional bucket into mandatory readiness cleanly.
            var mandatoryWeight = optional.Count > 0 ? 60.0 : 80.0;
            total += mandatoryWeight * ((double)matchedMandatoryCount / mandatory.Count);
        }

        if (optional.Count > 0)
        {
            total += 20.0 * ((double)matchedOptionalCount / optional.Count);
        }

        var totalMatchedSkills = matchedMandatoryCount + matchedOptionalCount;
        if (totalMatchedSkills > 0)
        {
            total += 10.0 * ((double)proficiencyMetCount / totalMatchedSkills);
        }

        var distinctInterests = studentInterests
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var haystack = $"{projectTitle} {projectDescription}";
        var matchedInterests = distinctInterests
            .Where(interest => haystack.Contains(interest, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (distinctInterests.Count > 0 && matchedInterests.Count > 0)
        {
            var denominator = Math.Min(distinctInterests.Count, 3);
            total += 10.0 * Math.Min(1.0, (double)matchedInterests.Count / denominator);
        }

        var missingSkills = missingMandatory.Concat(missingOptional).ToList();
        var clamped = Math.Clamp((int)Math.Round(total, MidpointRounding.AwayFromZero), 0, 100);

        return (clamped, matchedSkills, missingSkills, matchedInterests);
    }
}
