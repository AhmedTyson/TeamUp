using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

// ---------- Identity module ----------
public class University : BaseEntity
{
    public int UniversityId { get; set; }
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string? Website { get; set; }
    public ICollection<Faculty> Faculties { get; set; } = [];
}

public class Faculty : BaseEntity
{
    public int FacultyId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int UniversityId { get; set; }
    public University University { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = [];
}

public class Student : BaseEntity
{
    public int StudentId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int AcademicYear { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;
    public StudentProfile? Profile { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public ICollection<StudentInterest> Interests { get; set; } = [];
    public ICollection<StudentSkill> Skills { get; set; } = [];
}

public class StudentInterest : BaseEntity
{
    public int StudentId { get; set; }
    public string Interest { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class StudentProfile : BaseEntity
{
    public int ProfileId { get; set; }
    public int StudentId { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public Student Student { get; set; } = null!;
}

// ---------- Skills & Projects module ----------
public class Skill : BaseEntity
{
    public int SkillId { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
}

public class StudentSkill : BaseEntity
{
    public int StudentId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public decimal YearsOfExperience { get; set; }
    public Student Student { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}

public class Project : BaseEntity
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.Recruiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int MaxTeamSize { get; set; }
    public int CreatorId { get; set; }
    public Student Creator { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<ProjectTask> Tasks { get; set; } = [];
}

public class ProjectRequiredSkill : BaseEntity
{
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel RequiredLevel { get; set; }
    public bool IsMandatory { get; set; } = true;
    public Project Project { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public int StudentId { get; set; }
    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Project Project { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class ProjectApplication : BaseEntity
{
    public int ApplicationId { get; set; }
    public int ProjectId { get; set; }
    public int StudentId { get; set; }
    public string? Message { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public Project Project { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class ProjectTask : BaseEntity
{
    public int TaskId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int ProjectId { get; set; }
    public int? AssignedStudentId { get; set; }
    public Project Project { get; set; } = null!;
    public Student? AssignedStudent { get; set; }
}

// ---------- Organizations & Events module ----------
public class Organization : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? LogoUrl { get; set; }
}

public class OrganizationMember : BaseEntity
{
    public int OrganizationId { get; set; }
    public int StudentId { get; set; }
    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Organization Organization { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class MembershipApplication : BaseEntity
{
    public int ApplicationId { get; set; }
    public int OrganizationId { get; set; }
    public int StudentId { get; set; }
    public string? Message { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public Organization Organization { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class Event : BaseEntity
{
    public int EventId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventType Type { get; set; }
    public string Location { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public ICollection<EventSession> Sessions { get; set; } = [];
}

public class EventRegistration : BaseEntity
{
    public int EventId { get; set; }
    public int StudentId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;
    public Event Event { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class EventSession : BaseEntity
{
    public int SessionId { get; set; }
    public int EventId { get; set; }
    public int SessionNo { get; set; }
    public string Title { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Room { get; set; }
    public Event Event { get; set; } = null!;
}

public class SessionAttendance : BaseEntity
{
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public VerificationMethod VerificationMethod { get; set; }
    public EventSession Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

// ---------- Gamification & Notifications module ----------
public class ScoreTransaction : IAppendOnly
{
    public int TransactionId { get; set; }
    public int StudentId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = null!;
    public ScoreSourceType SourceType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Student Student { get; set; } = null!;
}

public class Badge : BaseEntity
{
    public int BadgeId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
    public BadgeCriteriaType CriteriaType { get; set; }
    public int CriteriaValue { get; set; }
}

public class StudentBadge : BaseEntity
{
    public int StudentId { get; set; }
    public int BadgeId { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    public Student Student { get; set; } = null!;
    public Badge Badge { get; set; } = null!;
}

public class Notification : BaseEntity
{
    public int NotificationId { get; set; }
    public int StudentId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Student Student { get; set; } = null!;
}
