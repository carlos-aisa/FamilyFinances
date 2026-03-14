using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Web.Features.Reports.Charts;

public static class AnnualChartDatasetAdapter
{
    public static IReadOnlyList<AnnualChartSeries> BuildEndBalanceSeries(
        MonthlyEvolutionReportDto report,
        string key = "end-balance",
        string label = "End Balance",
        string? colorHex = null)
    {
        var months = GetOrderedMonths(report);
        if (months.Count == 0)
            return Array.Empty<AnnualChartSeries>();

        var endBalancePoints = months
            .Select(month => new AnnualChartPoint(month, SumMonth(report, month, p => p.EndBalanceCents)))
            .ToList();

        return
        [
            new AnnualChartSeries(
                Key: key,
                Label: label,
                ColorHex: colorHex ?? ChartSemanticPalette.ResolveForSeriesKey(key, fallbackIndex: 0),
                Points: endBalancePoints)
        ];
    }

    public static IReadOnlyList<AnnualChartSeries> BuildSeriesEvolution(
        MonthlyEvolutionReportDto report,
        int maxSeries = 12,
        IReadOnlySet<string>? includedSeriesKeys = null)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualChartSeries>();

        return report.Series
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.SeriesKey, StringComparer.Ordinal)
            .Where(s => includedSeriesKeys is null || includedSeriesKeys.Contains(s.SeriesKey))
            .Take(Math.Max(1, maxSeries))
            .Select((series, idx) => new AnnualChartSeries(
                Key: series.SeriesKey,
                Label: series.DisplayName,
                ColorHex: ChartSemanticPalette.ResolveForSeriesKey(series.SeriesKey, idx),
                Points: series.Points
                    .OrderBy(p => p.Month)
                    .Select(p => new AnnualChartPoint(p.Month, p.EndBalanceCents))
                    .ToList()))
            .ToList();
    }

    public static IReadOnlyList<AnnualChartSeries> BuildSeriesMonthlyDelta(
        MonthlyEvolutionReportDto report,
        int maxSeries = 12,
        IReadOnlySet<string>? includedSeriesKeys = null)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualChartSeries>();

        return report.Series
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.SeriesKey, StringComparer.Ordinal)
            .Where(s => includedSeriesKeys is null || includedSeriesKeys.Contains(s.SeriesKey))
            .Take(Math.Max(1, maxSeries))
            .Select((series, idx) => new AnnualChartSeries(
                Key: series.SeriesKey,
                Label: series.DisplayName,
                ColorHex: ChartSemanticPalette.ResolveForSeriesKey(series.SeriesKey, idx),
                Points: series.Points
                    .OrderBy(p => p.Month)
                    .Select(p => new AnnualChartPoint(p.Month, p.DeltaVsPreviousMonthCents))
                    .ToList()))
            .ToList();
    }

    public static IReadOnlyList<AnnualCompositionSlice> BuildCompositionByNature(
        MonthlyEvolutionReportDto report,
        IReadOnlyDictionary<Guid, AccountNature> accountNatureById,
        AccountNature nature)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var weighted = report.Series
            .Where(s => s.EntityId is not null &&
                        accountNatureById.TryGetValue(s.EntityId.Value, out var foundNature) &&
                        foundNature == nature)
            .Select(s => new
            {
                s.SeriesKey,
                s.DisplayName,
                ValueCents = Math.Abs(GetLatestValueCents(s))
            })
            .Where(x => x.ValueCents > 0)
            .OrderByDescending(x => x.ValueCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (weighted.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var total = weighted.Sum(x => x.ValueCents);
        if (total == 0)
            return Array.Empty<AnnualCompositionSlice>();

        return weighted
            .Select((x, idx) => new AnnualCompositionSlice(
                Key: x.SeriesKey,
                Label: x.DisplayName,
                RawValueCents: x.ValueCents,
                Percentage: (x.ValueCents * 100m) / total,
                ColorHex: ChartSemanticPalette.ResolveIndexed(idx)))
            .ToList();
    }

    public static IReadOnlyList<AnnualCompositionSlice> BuildCompositionByNatureAtMonth(
        MonthlyEvolutionReportDto report,
        IReadOnlyDictionary<Guid, AccountNature> accountNatureById,
        AccountNature nature,
        int month)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var weighted = report.Series
            .Where(s => s.EntityId is not null &&
                        accountNatureById.TryGetValue(s.EntityId.Value, out var foundNature) &&
                        foundNature == nature)
            .Select(s => new
            {
                s.SeriesKey,
                s.DisplayName,
                ValueCents = Math.Abs(GetValueCentsAtOrBeforeMonth(s, month))
            })
            .Where(x => x.ValueCents > 0)
            .OrderByDescending(x => x.ValueCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (weighted.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var total = weighted.Sum(x => x.ValueCents);
        if (total == 0)
            return Array.Empty<AnnualCompositionSlice>();

        return weighted
            .Select((x, idx) => new AnnualCompositionSlice(
                Key: x.SeriesKey,
                Label: x.DisplayName,
                RawValueCents: x.ValueCents,
                Percentage: (x.ValueCents * 100m) / total,
                ColorHex: ChartSemanticPalette.ResolveIndexed(idx)))
            .ToList();
    }

    public static IReadOnlyList<AnnualCompositionSlice> BuildCompositionFromSeries(
        MonthlyEvolutionReportDto report,
        Func<MonthlyEvolutionSeriesDto, bool> predicate)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var weighted = report.Series
            .Where(predicate)
            .Select(s => new
            {
                s.SeriesKey,
                s.DisplayName,
                ValueCents = Math.Abs(GetLatestValueCents(s))
            })
            .Where(x => x.ValueCents > 0)
            .OrderByDescending(x => x.ValueCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (weighted.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var total = weighted.Sum(x => x.ValueCents);
        if (total == 0)
            return Array.Empty<AnnualCompositionSlice>();

        return weighted
            .Select((x, idx) => new AnnualCompositionSlice(
                Key: x.SeriesKey,
                Label: x.DisplayName,
                RawValueCents: x.ValueCents,
                Percentage: (x.ValueCents * 100m) / total,
                ColorHex: ChartSemanticPalette.ResolveIndexed(idx)))
            .ToList();
    }

    public static IReadOnlyList<AnnualCompositionSlice> BuildCompositionFromSeriesAtMonth(
        MonthlyEvolutionReportDto report,
        Func<MonthlyEvolutionSeriesDto, bool> predicate,
        int month)
    {
        if (report.Series.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var weighted = report.Series
            .Where(predicate)
            .Select(s => new
            {
                s.SeriesKey,
                s.DisplayName,
                ValueCents = Math.Abs(GetValueCentsAtOrBeforeMonth(s, month))
            })
            .Where(x => x.ValueCents > 0)
            .OrderByDescending(x => x.ValueCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (weighted.Count == 0)
            return Array.Empty<AnnualCompositionSlice>();

        var total = weighted.Sum(x => x.ValueCents);
        if (total == 0)
            return Array.Empty<AnnualCompositionSlice>();

        return weighted
            .Select((x, idx) => new AnnualCompositionSlice(
                Key: x.SeriesKey,
                Label: x.DisplayName,
                RawValueCents: x.ValueCents,
                Percentage: (x.ValueCents * 100m) / total,
                ColorHex: ChartSemanticPalette.ResolveIndexed(idx)))
            .ToList();
    }

    private static IReadOnlyList<int> GetOrderedMonths(MonthlyEvolutionReportDto report)
    {
        return report.Series
            .SelectMany(s => s.Points.Select(p => p.Month))
            .Distinct()
            .OrderBy(m => m)
            .ToList();
    }

    private static decimal SumMonth(
        MonthlyEvolutionReportDto report,
        int month,
        Func<MonthlyEvolutionPointDto, long> selector)
    {
        return report.Series
            .Select(s => s.Points.FirstOrDefault(p => p.Month == month))
            .Where(p => p is not null)
            .Select(p => selector(p!))
            .Sum();
    }

    private static long GetLatestValueCents(MonthlyEvolutionSeriesDto series)
    {
        return GetValueCentsAtOrBeforeMonth(series, month: int.MaxValue);
    }

    private static long GetValueCentsAtOrBeforeMonth(MonthlyEvolutionSeriesDto series, int month)
    {
        var point = series.Points
            .Where(p => p.Month <= month)
            .OrderByDescending(p => p.Month)
            .FirstOrDefault();

        return point?.EndBalanceCents ?? 0;
    }
}
