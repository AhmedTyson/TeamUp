// Date: 2026-11-25
// Purpose: Loads and validates the embedded Egyptian campus ecosystem JSON dataset with strict integrity and SSRF guards.

using System.Text.Json;
using System.Text.Json.Serialization;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Seed.Models;

namespace TeamUp.Infrastructure.Seed;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(EgyptCampusSeedData))]
internal sealed partial class SeedJsonSerializerContext : JsonSerializerContext;

internal static class SeedDataLoader
{
    private const string ResourceName = "TeamUp.Infrastructure.Seed.Data.egypt-campus-seed.json";

    private static readonly Lazy<EgyptCampusSeedData> CachedData = new(LoadAndValidateInternal);

    public static EgyptCampusSeedData Load() => CachedData.Value;

    private static EgyptCampusSeedData LoadAndValidateInternal()
    {
        var assembly = typeof(SeedDataLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded seed resource '{ResourceName}' was not found.");

        var data = JsonSerializer.Deserialize(stream, SeedJsonSerializerContext.Default.EgyptCampusSeedData)
            ?? throw new InvalidOperationException($"Embedded seed resource '{ResourceName}' deserialized to null.");

        Validate(data);
        return data;
    }

    private static void Validate(EgyptCampusSeedData data)
    {
        if (string.IsNullOrWhiteSpace(data.Version))
            throw new InvalidOperationException("Seed dataset version is required.");

        HashSet<string> uniNames = [with(StringComparer.OrdinalIgnoreCase)];
        foreach (var uni in data.Universities)
        {
            RequireLength(uni.Name, 150, $"University.Name:{uni.Name}");
            RequireLength(uni.City, 100, $"University.City:{uni.Name}");
            RequireLength(uni.Country, 100, $"University.Country:{uni.Name}");
            ValidateSafeHttpsUrl(uni.Website, 200, $"University.Website:{uni.Name}");
            ValidateSafeHttpsUrl(uni.LogoUrl, 500, $"University.LogoUrl:{uni.Name}");

            if (!uniNames.Add(uni.Name))
                throw new InvalidOperationException($"Duplicate seeded university name '{uni.Name}'.");

            HashSet<string> facCodes = [with(StringComparer.OrdinalIgnoreCase)];
            foreach (var fac in uni.Faculties)
            {
                RequireLength(fac.Name, 150, $"Faculty.Name:{fac.Code}");
                RequireLength(fac.Code, 20, $"Faculty.Code:{fac.Code}");
                if (!facCodes.Add(fac.Code))
                    throw new InvalidOperationException($"Duplicate faculty code '{fac.Code}' in '{uni.Name}'.");
            }
        }

        var studentEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var stu in data.Students)
        {
            RequireLength(stu.Email, 150, $"Student.Email:{stu.Email}");
            RequireLength(stu.FirstName, 50, $"Student.FirstName:{stu.Email}");
            RequireLength(stu.LastName, 50, $"Student.LastName:{stu.Email}");
            if (!Enum.TryParse<AffiliationType>(stu.AffiliationType, true, out _))
                throw new InvalidOperationException($"Invalid AffiliationType '{stu.AffiliationType}' for '{stu.Email}'.");
            ValidateSafeAssetOrHttpsUrl(stu.PhotoUrl, 300, $"Student.PhotoUrl:{stu.Email}");
            ValidateSafeHttpsUrl(stu.GitHubUrl, 200, $"Student.GitHubUrl:{stu.Email}");
            ValidateSafeHttpsUrl(stu.LinkedInUrl, 200, $"Student.LinkedInUrl:{stu.Email}");

            if (!studentEmails.Add(stu.Email))
                throw new InvalidOperationException($"Duplicate seeded student email '{stu.Email}'.");

            foreach (var interest in stu.Interests)
                RequireLength(interest, 50, $"StudentInterest:{stu.Email}");
        }

        var orgNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var org in data.Organizations)
        {
            RequireLength(org.Name, 120, $"Organization.Name:{org.Name}");
            RequireLength(org.Category, 50, $"Organization.Category:{org.Name}");
            ValidateSafeHttpsUrl(org.LogoUrl, 300, $"Organization.LogoUrl:{org.Name}");
            ValidateSafeHttpsUrl(org.WebsiteUrl, 300, $"Organization.WebsiteUrl:{org.Name}");
            if (!Enum.TryParse<OrganizationScope>(org.Scope, true, out _))
                throw new InvalidOperationException($"Invalid OrganizationScope '{org.Scope}' for '{org.Name}'.");

            if (!orgNames.Add(org.Name))
                throw new InvalidOperationException($"Duplicate seeded organization '{org.Name}'.");

            foreach (var unit in org.Units)
            {
                RequireLength(unit.Name, 100, $"OrganizationUnit.Name:{org.Name}/{unit.Code}");
                RequireLength(unit.Code, 20, $"OrganizationUnit.Code:{org.Name}/{unit.Code}");
                if (!Enum.TryParse<OrganizationUnitType>(unit.UnitType, true, out _))
                    throw new InvalidOperationException($"Invalid OrganizationUnitType '{unit.UnitType}'.");
            }
        }

        foreach (var proj in data.Projects)
        {
            RequireLength(proj.Title, 150, $"Project.Title:{proj.Title}");
            if (proj.MaxTeamSize < 1)
                throw new InvalidOperationException($"Invalid MaxTeamSize on project '{proj.Title}'.");
            if (proj.Members.Count > proj.MaxTeamSize)
                throw new InvalidOperationException($"Project '{proj.Title}' exceeds MaxTeamSize.");
        }

        foreach (var evt in data.Events)
        {
            RequireLength(evt.Title, 150, $"Event.Title:{evt.Title}");
            RequireLength(evt.Location, 200, $"Event.Location:{evt.Title}");
            ValidateSafeHttpsUrl(evt.MeetingUrl, 500, $"Event.MeetingUrl:{evt.Title}");
            if (evt.DurationHours < 1)
                throw new InvalidOperationException($"Invalid DurationHours on event '{evt.Title}'.");
            if (evt.Registrations.Count > evt.Capacity)
                throw new InvalidOperationException($"Event '{evt.Title}' exceeds Capacity.");
            foreach (var session in evt.Sessions)
            {
                RequireLength(session.Title, 120, $"EventSession.Title:{evt.Title}/{session.SessionNo}");
                if (session.DurationHours < 1)
                    throw new InvalidOperationException($"Invalid DurationHours on session '{session.Title}'.");
            }
        }
    }

    private static void RequireLength(string? value, int maxLen, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required seed field '{field}' is empty.");
        if (value.Length > maxLen)
            throw new InvalidOperationException($"Seed field '{field}' exceeds max length {maxLen} ({value.Length}).");
    }

    private static void ValidateSafeAssetOrHttpsUrl(string? url, int maxLen, string field)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        if (url.StartsWith("/images/", StringComparison.Ordinal) || url.StartsWith("/badges/", StringComparison.Ordinal))
        {
            if (url.Length > maxLen)
                throw new InvalidOperationException($"Seed path '{field}' exceeds max length {maxLen}.");
            return;
        }
        ValidateSafeHttpsUrl(url, maxLen, field);
    }

    private static void ValidateSafeHttpsUrl(string? url, int maxLen, string field)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        if (url.Length > maxLen)
            throw new InvalidOperationException($"Seed URL '{field}' exceeds max length {maxLen} ({url.Length}).");
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Seed URL '{field}' must be an absolute HTTPS URI: '{url}'.");
        }
        if (uri.IsLoopback || uri.Host.StartsWith("169.254.", StringComparison.Ordinal) || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Seed URL '{field}' targets restricted host '{uri.Host}'.");
    }
}
