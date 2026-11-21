// Date: 2026-11-21
// Purpose: Embedded HTML template renderer for non-UI transactional emails and printable attendance reports.

using System.Net;
using System.Reflection;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Infrastructure.Services;

public sealed class TemplateRenderer : ITemplateRenderer
{
    private static readonly Assembly Assembly = typeof(TemplateRenderer).Assembly;

    public async Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken ct = default)
    {
        var template = await LoadTemplateAsync("Emails", templateFileName, ct);
        foreach (var (key, value) in values)
        {
            template = template.Replace($"{{{{{key}}}}}", WebUtility.HtmlEncode(value), StringComparison.Ordinal);
        }
        return template;
    }

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
