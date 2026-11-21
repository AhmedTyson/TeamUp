// Date: 2026-11-21
// Purpose: Non-UI template renderer contract for transactional emails and printable HTML reports housed in TeamUp.Infrastructure/Templates.

namespace TeamUp.Application.Common.Interfaces;

public interface ITemplateRenderer
{
    Task<string> RenderEmailAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> values,
        CancellationToken ct = default);

    Task<string> RenderReportAsync(
        string templateFileName,
        IReadOnlyDictionary<string, string> encodedValues,
        CancellationToken ct = default);
}
