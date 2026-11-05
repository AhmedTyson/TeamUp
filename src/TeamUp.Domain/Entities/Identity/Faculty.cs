using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class Faculty : BaseEntity
{
    public int FacultyId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int UniversityId { get; set; }
    public University University { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = [];
}
