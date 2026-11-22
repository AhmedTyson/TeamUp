// Date: 2026-11-22
// Purpose: Magic-byte image signature validation preventing spoofed MIME/extension uploads (JPEG, PNG, WebP up to 2 MB).

namespace TeamUp.Application.Common.Validation;

public static class ImageFileValidator
{
    public const long MaxProfilePhotoBytes = 2 * 1024 * 1024;

    public static bool TryDetectImageExtension(
        ReadOnlySpan<byte> headerBytes,
        out string extension,
        out string contentType)
    {
        // JPEG: FF D8 FF
        if (headerBytes.Length >= 3
            && headerBytes[0] == 0xFF
            && headerBytes[1] == 0xD8
            && headerBytes[2] == 0xFF)
        {
            extension = ".jpg";
            contentType = "image/jpeg";
            return true;
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (headerBytes.Length >= 8
            && headerBytes[0] == 0x89
            && headerBytes[1] == 0x50
            && headerBytes[2] == 0x4E
            && headerBytes[3] == 0x47
            && headerBytes[4] == 0x0D
            && headerBytes[5] == 0x0A
            && headerBytes[6] == 0x1A
            && headerBytes[7] == 0x0A)
        {
            extension = ".png";
            contentType = "image/png";
            return true;
        }

        // WebP: RIFF....WEBP (bytes 0..3 == "RIFF", bytes 8..11 == "WEBP")
        if (headerBytes.Length >= 12
            && headerBytes[0] == 0x52
            && headerBytes[1] == 0x49
            && headerBytes[2] == 0x46
            && headerBytes[3] == 0x46
            && headerBytes[8] == 0x57
            && headerBytes[9] == 0x45
            && headerBytes[10] == 0x42
            && headerBytes[11] == 0x50)
        {
            extension = ".webp";
            contentType = "image/webp";
            return true;
        }

        extension = string.Empty;
        contentType = string.Empty;
        return false;
    }
}
