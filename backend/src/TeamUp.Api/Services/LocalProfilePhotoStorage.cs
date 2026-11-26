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
        var validated = await ImageFileValidator.ReadAndValidateImageAsync(content, contentLength, "Profile photo", ct);

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;

        var peopleDir = Path.Combine(webRoot, "images", "people");
        var peopleDirFull = Path.GetFullPath(peopleDir);
        Directory.CreateDirectory(peopleDirFull);

        var fileName = $"student-{studentId}-{Guid.NewGuid():N}{validated.Extension}";
        var fullPath = Path.GetFullPath(Path.Combine(peopleDirFull, fileName));
        if (!fullPath.StartsWith(peopleDirFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Invalid target photo path.");

        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await fs.WriteAsync(validated.Bytes, ct);
        }

        return $"/images/people/{fileName}";
    }
}
