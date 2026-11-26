// Date: 2026-11-21
// Purpose: Strongly-typed configuration options for email transport, admin governance, and rate limiting.

using System.ComponentModel.DataAnnotations;

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
    public string FromAddress
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = "no-reply@teamup.edu";

    public string FromName
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = "TeamUp Platform";

    public string ResetPasswordUrlBase
    {
        get;
        set => field = value?.Trim().TrimEnd('/') ?? string.Empty;
    } = "https://teamup.edu/reset-password";

    public string FrontendBaseUrl
    {
        get;
        set => field = value?.Trim().TrimEnd('/') ?? string.Empty;
    } = "https://teamup-ieee.vercel.app";
}

public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    /// <summary>
    /// Guards STRIDE THR-E-03: blocks runtime elevation to PlatformAdmin unless explicitly opted in.
    /// PlatformAdmin accounts should normally be provisioned via bootstrap configuration / DB seeders.
    /// </summary>
    public bool AllowRuntimePlatformAdminPromotion { get; set; }

    /// <summary>
    /// How many of the most recently soft-deleted rows the admin overview previews per entity type.
    /// Keeps the admin landing query bounded as the recycle-bin grows.
    /// </summary>
    [Range(1, 10000)]
    public int SoftDeletedPreviewLimit { get; set; } = 200;
}

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 100000)]
    public int AuthPermitLimit { get; set; } = 30;

    [Range(1, 100000)]
    public int AuthenticatedPermitLimit { get; set; } = 120;

    [Range(1, 100000)]
    public int AnonymousPermitLimit { get; set; } = 30;

    [Range(1, 100000)]
    public int DirectoryPermitLimit { get; set; } = 30;

    [Range(1, 100000)]
    public int UploadPermitLimit { get; set; } = 10;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;
}
