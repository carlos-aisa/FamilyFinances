namespace FamilyFinances.Application.Reporting.Dtos;

public enum DashboardDataSufficiencyState
{
    Complete = 1,
    Partial = 2,
    InsufficientHistory = 3
}

public sealed record DashboardKpiDto(
    long ValueCents,
    long DeltaVsPreviousMonthCents
);

public sealed record DashboardDailyIncomeExpensePointDto(
    int Day,
    long IncomeCents,
    long ExpenseCents,
    long NetCents
);

public sealed record DashboardGroupStatePointDto(
    string SeriesKey,
    string DisplayName,
    long SelectedMonthBalanceCents,
    long DeltaVsPreviousMonthCents
);

public sealed record DashboardMonthlyNetPointDto(
    int Month,
    long IncomeCents,
    long ExpenseCents,
    long NetCents,
    long AccumulatedNetCents
);

public sealed record DashboardYtdSummaryDto(
    long AccumulatedNetCents,
    IReadOnlyList<DashboardMonthlyNetPointDto> MonthlyNetPoints
);

public sealed record DashboardCompactInsightRowDto(
    string RowKey,
    string Kind,
    string Label,
    long AmountCents,
    decimal? Percentage,
    string? StatusCode
);

public sealed record DashboardOverviewDto(
    DateOnly AsOf,
    DateOnly SelectedMonthStart,
    DateOnly SelectedMonthEnd,
    DateOnly PreviousMonthStart,
    DateOnly PreviousMonthEnd,
    DashboardKpiDto Income,
    DashboardKpiDto Expense,
    DashboardKpiDto NetResult,
    DashboardKpiDto NetWorth,
    long? NetResultDeltaVsSameMonthLastYearCents,
    DashboardDataSufficiencyState DataSufficiencyState,
    IReadOnlyList<DashboardDailyIncomeExpensePointDto> DailyIncomeVsExpense,
    IReadOnlyList<DashboardGroupStatePointDto> GroupStates,
    DashboardYtdSummaryDto YtdSummary,
    IReadOnlyList<DashboardCompactInsightRowDto> CompactInsights
);

public sealed record DashboardOverviewCoreDto(
    DateOnly AsOf,
    DateOnly SelectedMonthStart,
    DateOnly SelectedMonthEnd,
    DateOnly PreviousMonthStart,
    DateOnly PreviousMonthEnd,
    EconomicStateDto CurrentState,
    EconomicStateDto PreviousState,
    IReadOnlyList<MonthlyChartPointDto> IncomeDailyPoints,
    IReadOnlyList<MonthlyChartPointDto> ExpenseDailyPoints,
    IReadOnlyList<DashboardMonthlyNetPointDto> MonthlyNetPoints,
    IReadOnlyList<DashboardGroupStatePointDto> GroupStates,
    bool HasPreviousMonthData,
    bool HasSameMonthLastYearData,
    long? SameMonthLastYearNetCents
);
