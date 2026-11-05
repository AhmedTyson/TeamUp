using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

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
