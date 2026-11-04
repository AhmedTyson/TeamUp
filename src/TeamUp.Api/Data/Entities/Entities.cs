using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamUp.Api.Data.Entities;

public class Organization
{
    [Key]
    public string OrganizationID { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrganizationMember> Members { get; set; } = [];
    public List<Event> Events { get; set; } = [];
}

public class Student
{
    [Key]
    public string StudentID { get; set; } = Guid.NewGuid().ToString();

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Student"; // "Student" | "PlatformAdmin"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrganizationMember> Memberships { get; set; } = [];
    public List<EventRegistration> Registrations { get; set; } = [];
    public List<SessionAttendance> Attendances { get; set; } = [];
    public List<ScoreTransaction> ScoreTransactions { get; set; } = [];
    public List<StudentBadge> Badges { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
}

public class OrganizationMember
{
    [Required]
    public string OrganizationID { get; set; } = string.Empty;
    public Organization Organization { get; set; } = null!;

    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Member"; // "OrgAdmin" | "Member"

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Event
{
    [Key]
    public string EventID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string OrganizationID { get; set; } = string.Empty;
    public Organization Organization { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Type { get; set; } = "Workshop"; // Workshop, Hackathon, Competition, Seminar, Bootcamp

    [Required, MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<EventSession> Sessions { get; set; } = [];
    public List<EventRegistration> Registrations { get; set; } = [];
}

public class EventSession
{
    [Key]
    public string SessionID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string EventID { get; set; } = string.Empty;
    public Event Event { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Speaker { get; set; }

    [Required, MaxLength(120)]
    public string Room { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsDeleted { get; set; } = false;

    public List<SessionAttendance> Attendances { get; set; } = [];
}

public class EventRegistration
{
    [Key]
    public string RegistrationID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string EventID { get; set; } = string.Empty;
    public Event Event { get; set; } = null!;

    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Confirmed"; // Confirmed, Waitlisted, Cancelled

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
}

public class SessionAttendance
{
    [Key]
    public string AttendanceID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string SessionID { get; set; } = string.Empty;
    public EventSession Session { get; set; } = null!;

    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string VerificationMethod { get; set; } = "Manual"; // QRCode, RFID, Manual

    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
}

public class ScoreTransaction
{
    [Key]
    public string TransactionID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    public int Points { get; set; } // Non-zero (+100, -100, bonus, etc.)

    [Required, MaxLength(255)]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string SourceType { get; set; } = "ManualAdjustment";

    [MaxLength(100)]
    public string? SourceID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Badge
{
    [Key]
    public string BadgeID { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string IconURL { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string CriteriaType { get; set; } = "TotalPoints"; // TotalPoints, EventsAttended, ProjectsCompleted, TasksCompleted, SkillsVerified, Manual

    public int CriteriaValue { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<StudentBadge> StudentBadges { get; set; } = [];
}

public class StudentBadge
{
    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    [Required]
    public string BadgeID { get; set; } = string.Empty;
    public Badge Badge { get; set; } = null!;

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    [Key]
    public string NotificationID { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string StudentID { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string Type { get; set; } = "PointsAwarded"; // EventRegistration, AttendanceVerified, PointsAwarded, BadgeUnlocked

    public bool IsRead { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    [MaxLength(150)]
    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

