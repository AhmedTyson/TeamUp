// Date: 2026-11-22
// Purpose: Stores validated student profile portraits in public wwwroot/images/people and rejects non-image uploads via magic bytes.

using Microsoft.AspNetCore.Hosting;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Api.Services;

public sealed class LocalProfilePhotoStorage(IWebHostEnvironment env) : IProfilePhotoStorage
{
    public async Task<string> SaveProfilePhotoAsync(
        int studentId,
        Stream content,
        long contentLength,
        CancellationToken ct = default)
    {
        if (contentLength <= 0 || contentLength > ImageFileValidator.MaxProfilePhotoBytes)
            throw new BadRequestException("Profile photo must be between 1 byte and 2 MB.");

        using var buffer = new MemoryStream((int)contentLength);
        var chunk = new byte[16 * 1024];
        long totalRead = 0;
        int read;
        while ((read = await content.ReadAsync(chunk, ct)) > 0)
        {
            totalRead += read;
            if (totalRead > ImageFileValidator.MaxProfilePhotoBytes)
                throw new BadRequestException("Profile photo must not exceed 2 MB.");

            buffer.Write(chunk, 0, read);
        }

        var bytes = buffer.GetBuffer().AsSpan(0, (int)totalRead);
        if (bytes.Length == 0 || !ImageFileValidator.TryDetectImageExtension(bytes, out var ext, out _))
            throw new BadRequestException("Only valid JPEG, PNG, or WebP images up to 2 MB are supported.");

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var peopleDir = Path.Combine(webRoot, "images", "people");
        Directory.CreateDirectory(peopleDir);

        var fileName = $"student-{studentId}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(peopleDir, fileName);

        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await fs.WriteAsync(buffer.GetBuffer().AsMemory(0, (int)totalRead), ct);
        }

        return $"/images/people/{fileName}";
    }
}
