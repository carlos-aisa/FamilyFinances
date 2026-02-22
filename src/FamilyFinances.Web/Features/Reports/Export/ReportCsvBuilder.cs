using System.Text;

namespace FamilyFinances.Web.Features.Reports.Export;

/// <summary>
/// Builds CSV payloads for reporting exports with optional filter/context metadata.
/// </summary>
public static class ReportCsvBuilder
{
    private const int MaxRows = 10_000;

    public static string Build(
        string reportName,
        IReadOnlyDictionary<string, string?> context,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string?>> rows)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"# Report: {reportName}");
        builder.AppendLine($"# ExportedAtUtc: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        foreach (var (key, value) in context.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            builder.AppendLine($"# {key}: {value}");
        }

        builder.AppendLine();
        builder.AppendLine(string.Join(",", headers.Select(EscapeCell)));

        var rowCount = 0;
        foreach (var row in rows)
        {
            if (rowCount >= MaxRows)
            {
                builder.AppendLine($"# WARNING: Output truncated to {MaxRows} rows.");
                break;
            }

            builder.AppendLine(string.Join(",", row.Select(EscapeCell)));
            rowCount++;
        }

        if (rowCount == 0)
        {
            builder.AppendLine("# No rows available for the selected filters.");
        }

        return builder.ToString();
    }

    private static string EscapeCell(string? value)
    {
        var normalized = value ?? string.Empty;
        var escaped = normalized.Replace("\"", "\"\"");
        var needsQuotes = escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r');
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
