// Date: 2026-11-21
// Purpose: Fail-fast startup security validator enforcing STRIDE THR-S-01, THR-S-03, and THR-I-03 in non-development environments.

using System.Buffers;
using System.Collections.Frozen;
using System.Data.Common;
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
    string? ConnectionString,
    string? EmailSmtpHost = null);

public static class SecurityStartupValidator
{
    #region Constants & Seed Password Validation

    private const int MinimumKeyBytes = 32;

    private static readonly SearchValues<string> ForbiddenKeyMarkers = SearchValues.Create(
        [
            "DevelopmentFallbackSecretKey",
            "DevelopmentOnlyLocalSigningKey",
            "MustBeOverriddenInProd",
            "changeme",
            "replace-me",
            "placeholder",
            "your-secret",
            "default-secret",
            "supersecretkey"
        ],
        StringComparison.OrdinalIgnoreCase);

    // Passwords that were published in repository docs/examples; never acceptable outside Development.
    private static readonly FrozenSet<string> PubliclyKnownSeedPasswords = new[]
    {
        "Admin@TeamUp2026!",
        "StudentPass123!",
        "DevelopmentSeedPassword2026!",
        "Adm!0RK8zvFIO2BDTX"
    }.ToFrozenSet(StringComparer.Ordinal);

    public static void ValidateSeedAdminPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 ||
            password.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase) ||
            PubliclyKnownSeedPasswords.Contains(password))
        {
            throw new InvalidOperationException(
                "CRITICAL: Seed:AdminPassword must be a unique, privately generated secret (at least 12 characters, not a documented sample value) when Seed:Enabled is true outside Development.");
        }
    }

    #endregion

    #region Startup Validation

    public static void Validate(SecurityStartupSettings settings)
    {
        ValidateJwtSettings(settings);

        if (!settings.IsProduction)
            return;

        ValidateProductionEmailSettings(settings);
        ValidateProductionDatabaseSettings(settings);
    }

    private static void ValidateJwtSettings(SecurityStartupSettings settings)
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

        if (!settings.IsDevelopment && settings.JwtKey.AsSpan().ContainsAny(ForbiddenKeyMarkers))
        {
            throw new InvalidOperationException(
                "CRITICAL: A secure, unique Jwt:Key must be configured outside Development. Default/fallback keys are forbidden.");
        }
    }

    private static void ValidateProductionEmailSettings(SecurityStartupSettings settings)
    {
        if (string.Equals(settings.EmailProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "CRITICAL: InMemory email provider is forbidden in Production (STRIDE THR-S-03). Configure Email:Provider=Smtp.");
        }

        if ((string.IsNullOrWhiteSpace(settings.EmailProvider)
                || settings.EmailProvider.Equals("Smtp", StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(settings.EmailSmtpHost))
        {
            throw new InvalidOperationException(
                "CRITICAL: Email:SmtpHost must be configured when Email:Provider=Smtp in Production (STRIDE THR-S-03).");
        }
    }

    private static void ValidateProductionDatabaseSettings(SecurityStartupSettings settings)
    {
        if (string.Equals(settings.DatabaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "CRITICAL: Sqlite DatabaseProvider is forbidden in Production. Configure SqlServer.");
        }

        var isSqlServer = string.IsNullOrWhiteSpace(settings.DatabaseProvider) ||
            settings.DatabaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

        if (isSqlServer
            && !string.IsNullOrWhiteSpace(settings.ConnectionString)
            && HasInsecureSqlTlsSetting(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "CRITICAL: Production SQL Server connections must enforce verified TLS (Encrypt=True;TrustServerCertificate=False) per STRIDE THR-I-03.");
        }
    }

    #endregion

    #region SQL TLS Inspection Helpers

    private static bool HasInsecureSqlTlsSetting(string connectionString)
    {
        DbConnectionStringBuilder builder;
        try
        {
            builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        }
        catch (ArgumentException)
        {
            // Malformed connection strings cannot guarantee verified TLS.
            return true;
        }

        if (TryGetConnectionSetting(builder, "TrustServerCertificate", "Trust Server Certificate", out var trustValue)
            && IsTruthySetting(trustValue))
        {
            return true;
        }

        if (TryGetConnectionSetting(builder, "Encrypt", null, out var encryptValue)
            && IsFalsySetting(encryptValue))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetConnectionSetting(
        DbConnectionStringBuilder builder,
        string primaryKey,
        string? aliasKey,
        out string value)
    {
        if (builder.TryGetValue(primaryKey, out var primary) && primary is not null)
        {
            value = primary.ToString()?.Trim() ?? string.Empty;
            return true;
        }

        if (aliasKey is not null && builder.TryGetValue(aliasKey, out var alias) && alias is not null)
        {
            value = alias.ToString()?.Trim() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool IsTruthySetting(string value) =>
        value.Equals("True", StringComparison.OrdinalIgnoreCase)
        || value == "1"
        || value.Equals("yes", StringComparison.OrdinalIgnoreCase);

    private static bool IsFalsySetting(string value) =>
        value.Equals("False", StringComparison.OrdinalIgnoreCase)
        || value == "0"
        || value.Equals("no", StringComparison.OrdinalIgnoreCase);

    #endregion
}
