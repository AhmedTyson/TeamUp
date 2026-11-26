// Date: 2026-11-21
// Purpose: Time-bound rotating HMAC-SHA256 attendance check-in token generator/verifier preventing attendance replay and spoofing (STRIDE THR-T-02).

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using Microsoft.Extensions.Options;

namespace TeamUp.Application.Common.Security;

public interface IAttendanceCodeService
{
    (string Code, DateTime ExpiresAtUtc) GenerateCode(int sessionId, DateTime? utcNow = null);
    bool ValidateCode(int sessionId, string? code, DateTime? utcNow = null);
}

public sealed class AttendanceCodeService(IOptions<JwtOptions> jwtOptions, TimeProvider? timeProvider = null)
    : IAttendanceCodeService
{
    public const int WindowSeconds = 60;
    private const int SignatureHexLength = 16;
    private readonly byte[] _signingKey = DerivePurposeKey(jwtOptions.Value.Key);
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public (string Code, DateTime ExpiresAtUtc) GenerateCode(int sessionId, DateTime? utcNow = null)
    {
        var now = utcNow ?? _time.GetUtcNow().UtcDateTime;
        var epochSeconds = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var step = epochSeconds / WindowSeconds;
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds((step + 1) * WindowSeconds).UtcDateTime;

        Span<char> signatureChars = stackalloc char[SignatureHexLength];
        WriteSignatureHex(sessionId, step, signatureChars);
        return ($"{step}.{signatureChars}", expiresAt);
    }

    public bool ValidateCode(int sessionId, string? code, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var span = code.AsSpan().Trim();
        var dotIndex = span.IndexOf('.');
        if (dotIndex <= 0)
            return false;

        var stepSpan = span[..dotIndex];
        var sigSpan = span[(dotIndex + 1)..];
        if (!long.TryParse(stepSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var step))
            return false;

        var now = utcNow ?? _time.GetUtcNow().UtcDateTime;
        var epochSeconds = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var currentStep = epochSeconds / WindowSeconds;

        // Strictly accept only the active 60-second window (no future steps, no expired windows).
        if (step != currentStep || sigSpan.Length != SignatureHexLength)
            return false;

        Span<char> expectedChars = stackalloc char[SignatureHexLength];
        WriteSignatureHex(sessionId, step, expectedChars);

        return CryptographicOperations.FixedTimeEquals(
            MemoryMarshal.AsBytes(expectedChars),
            MemoryMarshal.AsBytes(sigSpan));
    }

    private void WriteSignatureHex(int sessionId, long step, Span<char> destination)
    {
        Span<byte> payload = stackalloc byte[64];
        Utf8.TryWrite(payload, CultureInfo.InvariantCulture, $"session:{sessionId}:step:{step}", out var payloadLength);

        Span<byte> mac = stackalloc byte[32];
        HMACSHA256.TryHashData(_signingKey, payload[..payloadLength], mac, out _);
        Convert.TryToHexStringLower(mac[..8], destination, out _);
    }

    private static byte[] DerivePurposeKey(string baseKey)
    {
        var material = string.IsNullOrEmpty(baseKey) ? "dev-fallback-attendance-key" : baseKey;
        return SHA256.HashData(Encoding.UTF8.GetBytes($"TeamUp.AttendanceCheckIn.v1:{material}"));
    }
}
