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
    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;
    public StudentProfile? Profile { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public ICollection<StudentInterest> Interests { get; set; } = [];
    public ICollection<StudentSkill> Skills { get; set; } = [];
}
