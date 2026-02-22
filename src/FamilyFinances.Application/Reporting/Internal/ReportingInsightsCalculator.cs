using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Internal;

public sealed class ReportingInsightsCalculator : IReportingInsightsCalculator
{
    private const decimal ZScoreThreshold = 2.0m;
    private const decimal FlatThresholdMultiplier = 1.25m;

    public ReportingParetoInsightsDto BuildParetoInsights(
        DateOnly from,
        DateOnly to,
        ReportingInsightDimension dimension,
        int topN,
        IReadOnlyList<InsightContributorAggregateDto> expenseContributors,
        IReadOnlyList<InsightContributorAggregateDto> incomeContributors)
    {
        return new ReportingParetoInsightsDto(
            From: from,
            To: to,
            Dimension: dimension,
            Expense: BuildSection(AccountNature.Expense, topN, expenseContributors),
            Income: BuildSection(AccountNature.Income, topN, incomeContributors));
    }

    public ReportingAnomalyInsightsDto BuildMonthlyAnomalyInsights(
        int year,
        int month,
        AccountNature nature,
        ReportingInsightDimension dimension,
        int lookbackMonths,
        int requiredHistoryMonths,
        IReadOnlyList<InsightMonthlyContributorAggregateDto> monthlyContributors)
    {
        var targetMonth = new DateOnly(year, month, 1);
        var historyMonthStarts = BuildHistoryMonthStarts(targetMonth, lookbackMonths);
        var targetMonthKey = ToMonthKey(targetMonth);

        var grouped = monthlyContributors
            .GroupBy(x => new { x.EntityId, x.DisplayName })
            .Select(group =>
            {
                var amountsByMonth = group
                    .GroupBy(row => ToMonthKey(row.Year, row.Month))
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(x => Math.Abs(x.AmountCents)));

                var currentAmount = amountsByMonth.GetValueOrDefault(targetMonthKey, 0L);

                var historyValues = historyMonthStarts
                    .Select(start => amountsByMonth.GetValueOrDefault(ToMonthKey(start), 0L))
                    .ToList();

                var nonZeroHistoryCount = historyValues.Count(value => value > 0);
                var isInsufficientHistory = nonZeroHistoryCount < requiredHistoryMonths;

                var average = historyValues.Count == 0
                    ? 0m
                    : historyValues.Average(value => (decimal)value);

                var stdDev = CalculateStandardDeviation(historyValues, average);
                var threshold = CalculateThreshold(average, stdDev);
                decimal? zScore = stdDev > 0m
                    ? Round2((currentAmount - average) / stdDev)
                    : null;

                var isAnomaly = !isInsufficientHistory && currentAmount > RoundToLong(threshold);

                return new AnomalyContributorDto(
                    EntityId: group.Key.EntityId,
                    DisplayName: group.Key.DisplayName,
                    CurrentAmountCents: currentAmount,
                    BaselineMeanAmountCents: RoundToLong(average),
                    ThresholdAmountCents: RoundToLong(threshold),
                    ZScore: zScore,
                    IsAnomaly: isAnomaly,
                    IsInsufficientHistory: isInsufficientHistory,
                    HistoryMonthsCount: nonZeroHistoryCount,
                    Explanation: BuildExplanation(
                        isInsufficientHistory,
                        nonZeroHistoryCount,
                        requiredHistoryMonths,
                        average,
                        stdDev,
                        threshold));
            })
            .Where(x => x.CurrentAmountCents > 0 || x.HistoryMonthsCount > 0)
            .OrderByDescending(x => x.IsAnomaly)
            .ThenByDescending(x => x.CurrentAmountCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ReportingAnomalyInsightsDto(
            Year: year,
            Month: month,
            Nature: nature,
            Dimension: dimension,
            RequiredHistoryMonths: requiredHistoryMonths,
            ThresholdRule: "Anomaly if current amount is above baseline + 2σ (or baseline x1.25 when σ=0).",
            Contributors: grouped);
    }

    private static ParetoInsightSectionDto BuildSection(
        AccountNature nature,
        int topN,
        IReadOnlyList<InsightContributorAggregateDto> contributors)
    {
        var normalized = contributors
            .Select(contributor => new
            {
                contributor.EntityId,
                contributor.DisplayName,
                AmountCents = Math.Abs(contributor.AmountCents)
            })
            .Where(contributor => contributor.AmountCents > 0)
            .OrderByDescending(contributor => contributor.AmountCents)
            .ThenBy(contributor => contributor.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var totalAmount = normalized.Sum(contributor => contributor.AmountCents);
        var topContributors = normalized
            .Take(topN)
            .Select(contributor => new ParetoContributorDto(
                EntityId: contributor.EntityId,
                DisplayName: contributor.DisplayName,
                AmountCents: contributor.AmountCents,
                ContributionPercentage: totalAmount == 0
                    ? 0m
                    : Round2((decimal)contributor.AmountCents * 100m / totalAmount)))
            .ToList();

        var topNAmount = topContributors.Sum(x => x.AmountCents);
        var topNCoverage = totalAmount == 0
            ? 0m
            : Round2((decimal)topNAmount * 100m / totalAmount);

        return new ParetoInsightSectionDto(
            Nature: nature,
            TotalAmountCents: totalAmount,
            TopN: topN,
            TopNAmountCents: topNAmount,
            TopNCoveragePercentage: topNCoverage,
            Contributors: topContributors);
    }

    private static IReadOnlyList<DateOnly> BuildHistoryMonthStarts(DateOnly targetMonth, int lookbackMonths)
    {
        var result = new List<DateOnly>(lookbackMonths);
        for (var offset = lookbackMonths; offset >= 1; offset--)
            result.Add(targetMonth.AddMonths(-offset));

        return result;
    }

    private static decimal CalculateStandardDeviation(IReadOnlyList<long> values, decimal average)
    {
        if (values.Count == 0)
            return 0m;

        var variance = values
            .Select(value =>
            {
                var delta = value - average;
                return delta * delta;
            })
            .Average();

        return (decimal)Math.Sqrt((double)variance);
    }

    private static decimal CalculateThreshold(decimal average, decimal stdDev)
    {
        if (stdDev <= 0m)
            return average * FlatThresholdMultiplier;

        return average + (ZScoreThreshold * stdDev);
    }

    private static string BuildExplanation(
        bool isInsufficientHistory,
        int historyMonthsCount,
        int requiredHistoryMonths,
        decimal average,
        decimal stdDev,
        decimal threshold)
    {
        if (isInsufficientHistory)
            return $"Insufficient history ({historyMonthsCount}/{requiredHistoryMonths} active months).";

        if (stdDev <= 0m)
            return $"Flat baseline: threshold = baseline x {FlatThresholdMultiplier:0.##} ({RoundToLong(average)} -> {RoundToLong(threshold)} cents).";

        return $"Statistical baseline: threshold = baseline + 2σ ({RoundToLong(average)} + 2*{RoundToLong(stdDev)} cents).";
    }

    private static int ToMonthKey(DateOnly date) => ToMonthKey(date.Year, date.Month);

    private static int ToMonthKey(int year, int month) => (year * 100) + month;

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static long RoundToLong(decimal value) => (long)Math.Round(value, 0, MidpointRounding.AwayFromZero);
}
