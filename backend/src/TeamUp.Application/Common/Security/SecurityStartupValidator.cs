// Date: 2026-11-21
// Purpose: Fail-fast startup security validator enforcing STRIDE THR-S-01, THR-S-03, and THR-I-03 in non-development environments.

using System.Text;

namespace TeamUp.Application.Common.Security;

public sealed record SecurityStartupSettings(
    bool IsDevelopment,
    bool IsProduction,
    string? JwtKey,
    string? JwtIssuer,
    string? JwtAudience,
    string? EmailProvider,
    string? DatabaseProvider,
    string? ConnectionString);

public static class SecurityStartupValidator
{
    private const int MinimumKeyBytes = 32;

    private static readonly string[] ForbiddenKeyMarkers =
    [
        "DevelopmentFallbackSecretKey",
        "changeme",
        "replace-me",
        "placeholder",
        "your-secret",
        "default-secret",
        "supersecretkey"
    ];

    public static void Validate(SecurityStartupSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.JwtIssuer))
            throw new InvalidOperationException("CRITICAL: Jwt:Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(settings.JwtAudience))
            throw new InvalidOperationException("CRITICAL: Jwt:Audience must be configured.");

        if (string.IsNullOrWhiteSpace(settings.JwtKey) ||
            Encoding.UTF8.GetByteCount(settings.JwtKey) < MinimumKeyBytes)
        {
            throw new InvalidOperationException(
                $"CRITICAL: Jwt:Key must be configured and at least {MinimumKeyBytes} UTF-8 bytes long.");
        }

        if (!settings.IsDevelopment)
        {
            foreach (var marker in ForbiddenKeyMarkers)
            {
                if (settings.JwtKey.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "CRITICAL: A secure, unique Jwt:Key must be configured outside Development. Default/fallback keys are forbidden.");
                }
            }
        }

        if (settings.IsProduction)
        {
            if (string.Equals(settings.EmailProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "CRITICAL: InMemory email provider is forbidden in Production (STRIDE THR-S-03). Configure Email:Provider=Smtp.");
            }

            var isSqlServer = string.IsNullOrWhiteSpace(settings.DatabaseProvider) ||
                settings.DatabaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

            if (isSqlServer && !string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                if (HasInsecureSqlTlsSetting(settings.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "CRITICAL: Production SQL Server connections must enforce verified TLS (Encrypt=True;TrustServerCertificate=False) per STRIDE THR-I-03.");
                }
            }
        }
    }

    private static bool HasInsecureSqlTlsSetting(string connectionString)
    {
        var segments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            var idx = segment.IndexOf('=');
            if (idx <= 0) continue;

            var key = segment[..idx].Trim();
            var value = segment[(idx + 1)..].Trim();

            if (key.Equals("TrustServerCertificate", StringComparison.OrdinalIgnoreCase) &&
                (value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" || value.Equals("yes", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (key.Equals("Encrypt", StringComparison.OrdinalIgnoreCase) &&
                (value.Equals("False", StringComparison.OrdinalIgnoreCase) || value == "0" || value.Equals("no", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
