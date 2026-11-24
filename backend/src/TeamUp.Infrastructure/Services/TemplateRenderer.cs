// Date: 2026-11-21
// Purpose: Embedded HTML template renderer for non-UI transactional emails and printable attendance reports.

using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Infrastructure.Templates;

namespace TeamUp.Infrastructure.Services;

public sealed class TemplateRenderer(IOptions<EmailOptions>? emailOptions = null) : ITemplateRenderer
{
    private const string EmailResourcePrefix = "TeamUp.Infrastructure.Templates.Emails.";
    private static readonly Assembly Assembly = typeof(TemplateRenderer).Assembly;
    private static readonly Regex TokenPattern = new(
        @"\{\{\{([A-Za-z0-9_]+)\}\}\}|\{\{([A-Za-z0-9_]+)\}\}",
        RegexOptions.Compiled);
    private readonly EmailOptions? _emailOptions = emailOptions?.Value;

    public Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken ct = default) =>
        RenderEmailAsync(templateFileName, values, null, ct);

    public async Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> scalarValues,
        IReadOnlyDictionary<string, SafeHtmlFragment>? rawFragments,
        CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync("Emails", templateFileName, ct);
        return TokenPattern.Replace(template, match =>
        {
            if (match.Groups[1].Success)
            {
                var key = match.Groups[1].Value;
                if (rawFragments is not null && rawFragments.TryGetValue(key, out var fragment))
                    return fragment.Html;
                return match.Value;
            }

            if (match.Groups[2].Success)
            {
                var key = match.Groups[2].Value;
                if (scalarValues.TryGetValue(key, out var value))
                    return WebUtility.HtmlEncode(value);
                if (key == "BrandLogoUrl")
                    return WebUtility.HtmlEncode(BrandAssets.BuildPublicEmailLogoUrl(_emailOptions?.FrontendBaseUrl));
                return match.Value;
            }

            return match.Value;
        });
    }

    public IReadOnlyList<string> GetAvailableEmailTemplates() =>
        Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(EmailResourcePrefix, StringComparison.Ordinal)
                && n.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            .Select(n => n[EmailResourcePrefix.Length..])
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<string> RenderReportAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> encodedValues,
        CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync("Reports", templateFileName, ct);
        foreach (var (key, value) in encodedValues)
        {
            template = template.Replace($"{{{{{key}}}}}", value, StringComparison.Ordinal);
        }
        return template;
    }

    private static async Task<string> LoadTemplateAsync(string category, string fileName, CancellationToken ct)
    {
        var safeFileName = Path.GetFileName(fileName);
        var resourceName = $"TeamUp.Infrastructure.Templates.{category}.{safeFileName}";

        await using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded template '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(ct);
    }
}
