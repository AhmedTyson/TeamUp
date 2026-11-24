// Date: 2026-11-21
// Purpose: Non-UI template renderer contract for transactional emails and printable HTML reports housed in TeamUp.Infrastructure/Templates.

namespace TeamUp.Application.Common.Interfaces;

/// <summary>
/// Explicitly typed pre-encoded HTML fragment intended only for triple-brace {{{Key}}} template slots.
/// </summary>
public readonly record struct SafeHtmlFragment(string Html);

public interface ITemplateRenderer
{
    Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken ct = default);

    Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> scalarValues,
        IReadOnlyDictionary<string, SafeHtmlFragment>? rawFragments,
        CancellationToken ct = default);

    IReadOnlyList<string> GetAvailableEmailTemplates();

    Task<string> RenderReportAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> encodedValues,
        CancellationToken ct = default);
}
