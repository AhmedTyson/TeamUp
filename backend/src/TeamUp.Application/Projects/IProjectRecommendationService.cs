// Date: 2026-11-24
// Purpose: Freelancer-style portfolio-to-project recommendation contract scoring recruiting squads against a student's skills and interests.

namespace TeamUp.Application.Projects;

public sealed record ProjectMatchRecommendationDto(
    int ProjectId,
    string Title,
    string Description,
    int MatchPercentage,
    int CurrentMemberCount,
    int MaxTeamSize,
    int OpenSeats,
    string CreatorName,
    string UniversityName,
    string FacultyCode,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingSkills,
    IReadOnlyList<string> MatchedInterests);

public interface IProjectRecommendationService
{
    Task<IReadOnlyList<ProjectMatchRecommendationDto>> GetRecommendedProjectsForStudentAsync(
        int studentId,
        int maxResults = 5,
        int minMatchPercentage = 1,
        CancellationToken ct = default);
}
