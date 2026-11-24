using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Student : BaseEntity
{
    public int StudentId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int AcademicYear { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
    public bool ShowEmailInDirectory { get; set; }
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool ReceiveLeaderboardDigests { get; set; } = true;
    public bool ReceiveProjectMatchEmails { get; set; } = true;
    public AffiliationType AffiliationType { get; set; } = AffiliationType.UniversityStudent;
    public string? City { get; set; }
    public int? FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
    public StudentProfile? Profile { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    /// <summary>Hash of the immediately preceding refresh token; presenting it again signals token theft/replay.</summary>
    public string? PreviousRefreshToken { get; set; }
    public DateTime? PreviousRefreshTokenRotatedAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public ICollection<StudentInterest> Interests { get; set; } = [];
    public ICollection<StudentSkill> Skills { get; set; } = [];
}
