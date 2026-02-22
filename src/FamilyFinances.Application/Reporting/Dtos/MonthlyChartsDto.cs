namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record MonthlyChartPointDto(
    int Day,
    DateOnly Date,
    long EndBalanceCents
);

public sealed record MonthlyBalanceChartDto(
    int Year,
    int Month,
    IReadOnlyList<MonthlyChartPointDto> Points
);

public sealed record MonthlyChartSeriesDto(
    string SeriesKey,
    string DisplayName,
    Guid? EntityId,
    string EntityType,
    IReadOnlyList<MonthlyChartPointDto> Points
);

public sealed record MonthlyBalanceVsGroupsChartDto(
    int Year,
    int Month,
    IReadOnlyList<MonthlyChartSeriesDto> Series
);
