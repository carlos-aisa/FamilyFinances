using System.Text;
using Microsoft.JSInterop;

namespace FamilyFinances.Web.Features.Reports.Export;

/// <summary>
/// Shared JS interop helpers for report exports.
/// </summary>
public static class ReportExportInterop
{
    public static ValueTask DownloadCsvAsync(this IJSRuntime js, string fileName, string csvContent)
    {
        ArgumentNullException.ThrowIfNull(js);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return js.InvokeVoidAsync("familyFinancesCharts.downloadCsv", fileName, csvContent);
    }

    public static ValueTask DownloadChartImageAsync(this IJSRuntime js, string canvasId, string fileName)
    {
        ArgumentNullException.ThrowIfNull(js);
        ArgumentException.ThrowIfNullOrWhiteSpace(canvasId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return js.InvokeVoidAsync("familyFinancesCharts.downloadChartImage", canvasId, fileName);
    }

    public static string BuildFileName(string prefix, params object?[] segments)
    {
        var parts = new List<string> { SanitizeSegment(prefix) };
        parts.AddRange(segments
            .Where(segment => segment is not null)
            .Select(segment => SanitizeSegment(segment!.ToString()!))
            .Where(segment => !string.IsNullOrWhiteSpace(segment)));

        return string.Join("-", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var normalized = new StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            if (invalid.Contains(ch))
                continue;

            normalized.Append(char.IsWhiteSpace(ch) ? '-' : ch);
        }

        return normalized.ToString()
            .Replace("--", "-", StringComparison.Ordinal)
            .Trim('-');
    }
}
