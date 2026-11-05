using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

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
