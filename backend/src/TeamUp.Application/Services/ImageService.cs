// Date: 2026-11-25
// Purpose: Database-backed image storage. Uploads are validated by magic bytes (JPEG/PNG/WebP, 2 MB max, same
// rules as profile photos) and stored as rows, so images survive deployments without a static-file volume and
// can back any existing *Url field (organization logos/covers, event covers, project images).

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Images;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Validation;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Services;

public interface IImageService
{
    Task<StoredImageDto> SaveImageAsync(int studentId, Stream content, long contentLength, CancellationToken ct = default);
    Task<StoredImageContent?> GetImageAsync(Guid imageId, CancellationToken ct = default);
    Task<int> EnsureDefaultCoversAsync(string seedImagesDirectory, CancellationToken ct = default);
}

public sealed record StoredImageDto(Guid ImageId, string Url, string ContentType, long SizeBytes);
public sealed record StoredImageContent(byte[] Content, string ContentType);

public sealed class ImageService(IAppDbContext db, TimeProvider? timeProvider = null) : IImageService
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public async Task<StoredImageDto> SaveImageAsync(int studentId, Stream content, long contentLength, CancellationToken ct = default)
    {
        var validated = await ImageFileValidator.ReadAndValidateImageAsync(content, contentLength, "Image", ct);

        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var now = _time.GetUtcNow();
        var image = new StoredImage
        {
            StoredImageId = Guid.CreateVersion7(now),
            UploadedByStudentId = studentId,
            Content = validated.Bytes,
            ContentType = validated.ContentType,
            SizeBytes = validated.Bytes.LongLength,
            UploadedAt = now.UtcDateTime
        };
        db.StoredImages.Add(image);
        await db.SaveChangesAsync(ct);

        return new StoredImageDto(image.StoredImageId, $"/api/v1/images/{image.StoredImageId}", validated.ContentType, image.SizeBytes);
    }

    public async Task<StoredImageContent?> GetImageAsync(Guid imageId, CancellationToken ct = default)
    {
        var stored = await db.StoredImages.AsNoTracking()
            .Where(x => x.StoredImageId == imageId)
            .Select(x => new StoredImageContent(x.Content, x.ContentType))
            .FirstOrDefaultAsync(ct);
        if (stored is not null)
            return stored;

        // Default covers are also served from the copied SeedImages output directory when their seed row is missing
        // (e.g. a production database created before this feature), so a default cover URL can never 404.
        var cover = DefaultCoverCatalog.Find(imageId);
        if (cover is null)
            return null;

        var path = Path.Combine(AppContext.BaseDirectory, "SeedImages", cover.FileName);
        return File.Exists(path) ? new StoredImageContent(await File.ReadAllBytesAsync(path, ct), cover.ContentType) : null;
    }

    /// <summary>Inserts any missing seeded default covers. Idempotent; safe to run at every startup.</summary>
    public async Task<int> EnsureDefaultCoversAsync(string seedImagesDirectory, CancellationToken ct = default)
    {
        var existing = await db.StoredImages.AsNoTracking()
            .Where(x => x.UploadedByStudentId == null)
            .Select(x => x.StoredImageId)
            .ToListAsync(ct);
        var present = existing.ToHashSet();
        var added = 0;
        var now = _time.GetUtcNow().UtcDateTime;
        foreach (var cover in DefaultCoverCatalog.Covers)
        {
            if (present.Contains(cover.ImageId))
                continue;
            var path = Path.Combine(seedImagesDirectory, cover.FileName);
            if (!File.Exists(path))
                continue;
            var bytes = await File.ReadAllBytesAsync(path, ct);
            db.StoredImages.Add(new StoredImage
            {
                StoredImageId = cover.ImageId,
                UploadedByStudentId = null,
                Content = bytes,
                ContentType = cover.ContentType,
                SizeBytes = bytes.LongLength,
                UploadedAt = now
            });
            added++;
        }
        if (added > 0)
            await db.SaveChangesAsync(ct);
        return added;
    }
}
