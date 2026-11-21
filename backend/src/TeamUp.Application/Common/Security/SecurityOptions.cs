// Date: 2026-11-21
// Purpose: Strongly-typed configuration options for email transport, admin governance, and rate limiting.

namespace TeamUp.Application.Common.Security;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Supported values: "Smtp" (production default) or "InMemory" (development/testing only).</summary>
    public string Provider { get; set; } = "Smtp";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "no-reply@teamup.edu";
    public string FromName { get; set; } = "TeamUp Platform";
    public string ResetPasswordUrlBase { get; set; } = "https://teamup.edu/reset-password";
}

public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    /// <summary>
    /// Guards STRIDE THR-E-03: blocks runtime elevation to PlatformAdmin unless explicitly opted in.
    /// PlatformAdmin accounts should normally be provisioned via bootstrap configuration / DB seeders.
    /// </summary>
    public bool AllowRuntimePlatformAdminPromotion { get; set; }
}

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int AuthPermitLimit { get; set; } = 30;
    public int AuthenticatedPermitLimit { get; set; } = 120;
    public int AnonymousPermitLimit { get; set; } = 30;
    public int DirectoryPermitLimit { get; set; } = 30;
    public int WindowSeconds { get; set; } = 60;
}
