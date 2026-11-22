// Date: 2026-11-22
// Purpose: Storage abstraction for saving validated student profile portraits to public static hosting.

namespace TeamUp.Application.Common.Interfaces;

public interface IProfilePhotoStorage
{
    Task<string> SaveProfilePhotoAsync(
        int studentId,
        Stream content,
        long contentLength,
        CancellationToken ct = default);
}
