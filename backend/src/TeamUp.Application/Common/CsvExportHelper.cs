// Date: 2026-11-24
// Purpose: RFC-4180 CSV builder with formula-injection neutralization (=, +, -, @, \t, \r) for exported rosters and applications.

using System.Text;

namespace TeamUp.Application.Common;

public static class CsvExportHelper
{
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

        var value = raw.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');

        // Neutralize spreadsheet formula execution when opened in Excel / Google Sheets
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t')
            value = "'" + value;

        if (value.Contains(',') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
