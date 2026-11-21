// Date: 2026-11-21
// Purpose: Time-bound rotating HMAC-SHA256 attendance check-in token generator/verifier preventing attendance replay and spoofing (STRIDE THR-T-02).

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace TeamUp.Application.Common.Security;

public interface IAttendanceCodeService
{
    (string Code, DateTime ExpiresAtUtc) GenerateCode(int sessionId, DateTime? utcNow = null);
    bool ValidateCode(int sessionId, string? code, DateTime? utcNow = null);
}

public sealed class AttendanceCodeService(IOptions<JwtOptions> jwtOptions) : IAttendanceCodeService
{
    public const int WindowSeconds = 60;
    private readonly byte[] _signingKey = DerivePurposeKey(jwtOptions.Value.Key);

    public (string Code, DateTime ExpiresAtUtc) GenerateCode(int sessionId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        var epochSeconds = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var step = epochSeconds / WindowSeconds;
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds((step + 1) * WindowSeconds).UtcDateTime;

        var signature = ComputeSignature(sessionId, step);
        return ($"{step}.{signature}", expiresAt);
    }

    public bool ValidateCode(int sessionId, string? code, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var parts = code.Trim().Split('.', 2);
        if (parts.Length != 2 || !long.TryParse(parts[0], out var step))
            return false;

        var now = utcNow ?? DateTime.UtcNow;
        var epochSeconds = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc)).ToUnixTimeSeconds();
        var currentStep = epochSeconds / WindowSeconds;

        // Strictly accept only the active 60-second window (no future steps, no expired windows).
        if (step != currentStep)
            return false;

        var expected = ComputeSignature(sessionId, step);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(parts[1]);

        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private string ComputeSignature(int sessionId, long step)
    {
        var payload = Encoding.UTF8.GetBytes($"session:{sessionId}:step:{step}");
        var mac = HMACSHA256.HashData(_signingKey, payload);
        return Convert.ToHexStringLower(mac)[..16];
    }

    private static byte[] DerivePurposeKey(string baseKey)
    {
        var material = string.IsNullOrEmpty(baseKey) ? "dev-fallback-attendance-key" : baseKey;
        return SHA256.HashData(Encoding.UTF8.GetBytes($"TeamUp.AttendanceCheckIn.v1:{material}"));
    }
}
