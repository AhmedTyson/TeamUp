using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class StudentInterest : BaseEntity
{
    public int StudentId { get; set; }
    public string Interest { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
