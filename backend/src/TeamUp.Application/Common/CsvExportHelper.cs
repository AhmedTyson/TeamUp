// Date: 2026-11-24
// Purpose: RFC-4180 CSV builder with formula-injection neutralization (=, +, -, @, \t, \r) for exported rosters and applications.

using System.Buffers;
using System.Text;

namespace TeamUp.Application.Common;

public static class CsvExportHelper
{
    private static readonly SearchValues<char> NewLineChars = SearchValues.Create("\r\n");
    private static readonly SearchValues<char> FormulaPrefixChars = SearchValues.Create("=+-@\t");
    private static readonly SearchValues<char> QuotingChars = SearchValues.Create(",\"");

    public static string BuildCsv(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCell)));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(EscapeCell)));
        }
        return sb.ToString();
    }

    public static string EscapeCell(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
            return string.Empty;

        var rawSpan = raw.AsSpan();
        if (!rawSpan.ContainsAny(NewLineChars)
            && !FormulaPrefixChars.Contains(rawSpan[0])
            && !rawSpan.ContainsAny(QuotingChars))
        {
            return raw;
        }

        var value = rawSpan.ContainsAny(NewLineChars)
            ? raw.Replace("\r\n", " ", StringComparison.Ordinal).Replace('\r', ' ').Replace('\n', ' ')
            : raw;

        // Neutralize spreadsheet formula execution when opened in Excel / Google Sheets
        if (value.Length > 0 && FormulaPrefixChars.Contains(value[0]))
            value = "'" + value;

        if (value.AsSpan().ContainsAny(QuotingChars))
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

        return value;
    }
}
