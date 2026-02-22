using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Web.Features.Reports.Charts;

public static class MonthlyChartDatasetAdapter
{
    public static IReadOnlyList<MonthlyChartSeries> BuildSingleSeries(
        MonthlyBalanceChartDto chart,
        string key,
        string label,
        string? colorHex = null)
    {
        var points = chart.Points
            .OrderBy(p => p.Day)
            .Select(p => new MonthlyChartPoint(p.Day, p.EndBalanceCents))
            .ToList();

        if (points.Count == 0)
            return Array.Empty<MonthlyChartSeries>();

        return
        [
            new MonthlyChartSeries(
                key,
                label,
                colorHex ?? AnnualChartPalette.Resolve(0),
                points)
        ];
    }

    public static IReadOnlyList<MonthlyChartSeries> BuildSeries(
        MonthlyBalanceVsGroupsChartDto chart,
        int? maxSeries = null)
    {
        if (chart.Series.Count == 0)
            return Array.Empty<MonthlyChartSeries>();

        var projected = chart.Series
            .Select((series, idx) => new MonthlyChartSeries(
                series.SeriesKey,
                series.DisplayName,
                AnnualChartPalette.Resolve(idx),
                series.Points
                    .OrderBy(p => p.Day)
                    .Select(p => new MonthlyChartPoint(p.Day, p.EndBalanceCents))
                    .ToList()))
            .ToList();

        if (maxSeries is null || maxSeries.Value <= 0)
            return projected;

        return projected.Take(maxSeries.Value).ToList();
    }
}
