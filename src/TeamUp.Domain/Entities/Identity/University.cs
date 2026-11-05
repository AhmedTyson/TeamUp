using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class University : BaseEntity
{
    public int UniversityId { get; set; }
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string? Website { get; set; }
    public ICollection<Faculty> Faculties { get; set; } = [];
}
