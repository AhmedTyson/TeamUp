// Date: 2026-11-25
// Purpose: Database-stored image uploaded by a student (logos, covers, photos). Bytes live in the database so
// images survive deployments and work without a static-file volume; served back through GET /api/v1/images/{id}.

namespace TeamUp.Domain.Entities;

public class StoredImage
{
    public Guid StoredImageId { get; set; }
    /// <summary>Null for system-generated images (the seeded default covers), which belong to no student.</summary>
    public int? UploadedByStudentId { get; set; }
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Student? UploadedBy { get; set; }
}
