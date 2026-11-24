using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class MembershipApplication : BaseEntity
{
    public int ApplicationId { get; set; }
    public int OrganizationId { get; set; }
    public int StudentId { get; set; }
    public string? Message { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? OrganizationUnitId { get; set; }
    public string? SeasonName { get; set; }
    public RecruitmentStage Stage { get; set; } = RecruitmentStage.Applied;
    public int? ReviewerStudentId { get; set; }
    public decimal? EvaluationScore { get; set; }
    public string? InterviewNotes { get; set; }
    public Organization Organization { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public OrganizationUnit? Unit { get; set; }
}
