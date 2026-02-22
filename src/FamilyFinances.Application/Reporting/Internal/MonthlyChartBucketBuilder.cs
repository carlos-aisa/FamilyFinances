using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Application.Reporting.Internal;

public static class MonthlyChartBucketBuilder
{
    public static IReadOnlyList<MonthlyChartPointDto> BuildDailyEndBalancePoints(
        int year,
        int month,
        long openingBalanceCents,
        IReadOnlyDictionary<int, long> movementByDayCents)
    {
        ReportingGuards.EnsureValidYear(year);
        ReportingGuards.EnsureValidMonth(month);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var points = new List<MonthlyChartPointDto>(daysInMonth);
        var previousEnd = openingBalanceCents;

        for (var day = 1; day <= daysInMonth; day++)
        {
            var movement = movementByDayCents.GetValueOrDefault(day, 0L);
            var endBalance = previousEnd + movement;
            points.Add(new MonthlyChartPointDto(day, new DateOnly(year, month, day), endBalance));
            previousEnd = endBalance;
        }

        return points;
    }

    public static IReadOnlyList<MonthlyChartSeriesDto> AlignSeriesDayBuckets(
        int year,
        int month,
        IReadOnlyList<MonthlyChartSeriesDto> series)
    {
        ReportingGuards.EnsureValidYear(year);
        ReportingGuards.EnsureValidMonth(month);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var aligned = new List<MonthlyChartSeriesDto>(series.Count);

        foreach (var item in series)
        {
            var pointByDay = item.Points
                .OrderBy(p => p.Day)
                .GroupBy(p => p.Day)
                .ToDictionary(g => g.Key, g => g.Last().EndBalanceCents);

            var points = new List<MonthlyChartPointDto>(daysInMonth);
            var currentEndBalance = 0L;

            for (var day = 1; day <= daysInMonth; day++)
            {
                if (pointByDay.TryGetValue(day, out var explicitValue))
                    currentEndBalance = explicitValue;

                points.Add(new MonthlyChartPointDto(day, new DateOnly(year, month, day), currentEndBalance));
            }

            aligned.Add(item with { Points = points });
        }

        return aligned;
    }
}
