// Date: 2026-11-24
// Purpose: Deterministic portfolio-to-project recommendation engine ranking recruiting squads by mandatory skills (60%), optional skills (20%), proficiency (10%), and interest overlap (10%).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Projects;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class ProjectRecommendationService(IAppDbContext db) : IProjectRecommendationService
{
    #region Single-Student Recommendations

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
                UniversityName = p.Creator.Faculty != null ? p.Creator.Faculty.University.Name : (p.Creator.City != null && p.Creator.City != "" ? p.Creator.City + " Community" : "Egypt Open Community"),
                FacultyCode = p.Creator.Faculty != null ? p.Creator.Faculty.Code : "IND",
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

        var candidateRows = candidates
            .Select(c => new ProjectCandidateProjection(
                c.ProjectId, c.Title, c.Description, c.MaxTeamSize, c.CurrentMemberCount,
                c.CreatedAt, c.CreatorName, c.UniversityName, c.FacultyCode));

        return RankCandidates(candidateRows, studentSkills, studentInterests, skillsByProject, safeMinScore, safeLimit);
    }

    #endregion

    #region Batched Multi-Student Recommendations

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<ProjectMatchRecommendationDto>>> GetRecommendedProjectsForStudentsAsync(
        IReadOnlyCollection<int> studentIds,
        int maxResults = 5,
        int minMatchPercentage = 1,
        CancellationToken ct = default)
    {
        var safeLimit = Math.Clamp(maxResults, 1, 20);
        var safeMinScore = Math.Clamp(minMatchPercentage, 0, 100);
        var result = new Dictionary<int, IReadOnlyList<ProjectMatchRecommendationDto>>();
        if (studentIds.Count == 0)
            return result;

        // One read of each recipient's portfolio, one read of the whole recruiting pool with its members,
        // applicants and required skills: O(pool) queries total instead of O(recipients).
        var skillsByStudent = (await db.StudentSkills
                .AsNoTracking()
                .Where(ss => studentIds.Contains(ss.StudentId))
                .Select(ss => new { ss.StudentId, ss.SkillId, ss.Skill.Name, ss.ProficiencyLevel })
                .ToListAsync(ct))
            .GroupBy(x => x.StudentId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, (string SkillName, ProficiencyLevel Level)>)g
                    .ToDictionary(x => x.SkillId, x => (x.Name, x.ProficiencyLevel)));

        var interestsByStudent = (await db.StudentInterests
                .AsNoTracking()
                .Where(si => studentIds.Contains(si.StudentId))
                .Select(si => new { si.StudentId, si.Interest })
                .ToListAsync(ct))
            .GroupBy(x => x.StudentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Interest).ToList());

        var pool = await db.Projects
            .AsNoTracking()
            .Where(p => p.Status == ProjectStatus.Recruiting)
            .Select(p => new
            {
                p.ProjectId,
                p.Title,
                p.Description,
                p.MaxTeamSize,
                p.CreatorId,
                p.CreatedAt,
                CreatorName = p.Creator.FirstName + " " + p.Creator.LastName,
                UniversityName = p.Creator.Faculty != null ? p.Creator.Faculty.University.Name : (p.Creator.City != null && p.Creator.City != "" ? p.Creator.City + " Community" : "Egypt Open Community"),
                FacultyCode = p.Creator.Faculty != null ? p.Creator.Faculty.Code : "IND",
                MemberIds = p.Members.Select(m => m.StudentId).ToList()
            })
            .ToListAsync(ct);

        var poolIds = pool.Select(p => p.ProjectId).ToArray();

        var busyApplicantIds = (await db.ProjectApplications
                .AsNoTracking()
                .Where(a => poolIds.Contains(a.ProjectId)
                    && studentIds.Contains(a.StudentId)
                    && (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted))
                .Select(a => new { a.ProjectId, a.StudentId })
                .ToListAsync(ct))
            .GroupBy(x => x.ProjectId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<int>)g.Select(x => x.StudentId).ToList());

        var skillsByProject = (await db.ProjectRequiredSkills
                .AsNoTracking()
                .Where(prs => poolIds.Contains(prs.ProjectId))
                .Select(prs => new
                {
                    prs.ProjectId,
                    prs.SkillId,
                    SkillName = prs.Skill.Name,
                    prs.RequiredLevel,
                    prs.IsMandatory
                })
                .ToListAsync(ct))
            .GroupBy(r => r.ProjectId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<(int SkillId, string SkillName, ProficiencyLevel RequiredLevel, bool IsMandatory)>)g
                    .Select(x => (x.SkillId, x.SkillName, x.RequiredLevel, x.IsMandatory))
                    .ToList());

        foreach (var studentId in studentIds.Distinct())
        {
            var studentSkills = skillsByStudent.GetValueOrDefault(studentId)
                ?? new Dictionary<int, (string SkillName, ProficiencyLevel Level)>();
            var studentInterests = interestsByStudent.GetValueOrDefault(studentId)
                ?? (IReadOnlyList<string>)[];

            var eligible = pool
                .Where(project =>
                    project.CreatorId != studentId
                    && !project.MemberIds.Contains(studentId)
                    && project.MemberIds.Count < project.MaxTeamSize
                    && !(busyApplicantIds.TryGetValue(project.ProjectId, out var applicants) && applicants.Contains(studentId)))
                .Select(project => new ProjectCandidateProjection(
                    project.ProjectId, project.Title, project.Description, project.MaxTeamSize, project.MemberIds.Count,
                    project.CreatedAt, project.CreatorName, project.UniversityName, project.FacultyCode));

            result[studentId] = RankCandidates(eligible, studentSkills, studentInterests, skillsByProject, safeMinScore, safeLimit);
        }

        return result;
    }

    #endregion

    #region Ranking & Scoring Engine

    private sealed record ProjectCandidateProjection(
        int ProjectId,
        string Title,
        string Description,
        int MaxTeamSize,
        int CurrentMemberCount,
        DateTime CreatedAt,
        string CreatorName,
        string UniversityName,
        string FacultyCode);

    private static List<ProjectMatchRecommendationDto> RankCandidates(
        IEnumerable<ProjectCandidateProjection> candidates,
        IReadOnlyDictionary<int, (string SkillName, ProficiencyLevel Level)> studentSkills,
        IReadOnlyList<string> studentInterests,
        IReadOnlyDictionary<int, IReadOnlyList<(int SkillId, string SkillName, ProficiencyLevel RequiredLevel, bool IsMandatory)>> skillsByProject,
        int minScore,
        int limit)
    {
        var ranked = new List<(ProjectMatchRecommendationDto Dto, DateTime CreatedAt)>();

        foreach (var project in candidates)
        {
            var reqSkills = skillsByProject.GetValueOrDefault(project.ProjectId) ?? [];

            var (score, matchedSkills, missingSkills, matchedInterests) = ComputeMatchScore(
                studentSkills,
                studentInterests,
                reqSkills,
                project.Title,
                project.Description);

            if (score < minScore)
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
            .Take(limit)
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

        List<string> matchedSkills = [];
        List<string> missingMandatory = [];
        List<string> missingOptional = [];

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
            // Symmetric with the mandatory-only case above: when a project lists only optional skills, the whole
            // skills bucket goes to them. Otherwise an "all nice-to-have" project capped at 40/100 and could never
            // surface as a strong match even for a perfectly matching student.
            var optionalWeight = mandatory.Count > 0 ? 20.0 : 80.0;
            total += optionalWeight * ((double)matchedOptionalCount / optional.Count);
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

    #endregion
}
