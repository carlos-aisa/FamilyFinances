namespace FamilyFinances.Application.Reporting.Dtos;

public enum MonthlyEvolutionScope
{
    Accounts = 1,
    AssetTotal = 2,
    AccountGroups = 3
}

public sealed record MonthlyEvolutionReportDto(
    int Year,
    MonthlyEvolutionScope Scope,
    IReadOnlyList<MonthlyEvolutionSeriesDto> Series
);

public sealed record MonthlyEvolutionSeriesDto(
    string SeriesKey,
    string DisplayName,
    Guid? EntityId,
    string? EntityType,
    IReadOnlyList<MonthlyEvolutionPointDto> Points
);

public sealed record MonthlyEvolutionPointDto(
    int Month,
    DateOnly MonthEndDate,
    long EndBalanceCents,
    long DeltaVsPreviousMonthCents,
    long DeltaVsYearStartCents
);
