namespace FamilyFinances.Application.Reporting.Dtos;

public enum DashboardDataSufficiencyState
{
    Complete = 1,
    Partial = 2,
    InsufficientHistory = 3
}

public enum DashboardPinnedGroupMetricKind
{
    Expense,
    Income,
    NetResult
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

public sealed record DashboardExpenseKindTotalDto(
    Guid KindId,
    string KindName,
    long AmountCents);

public sealed record DashboardExpenseKindRankDto(
    Guid? KindId,
    string Label,
    long AmountCents,
    decimal Percentage,
    bool IsOthers);

public sealed record DashboardPinnedGroupOperationalResultDto(
    Guid GroupId,
    string GroupName,
    long MonthOperationalResultCents,
    long YtdOperationalResultCents,
    DashboardPinnedGroupMetricKind MetricKind);

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
    DashboardKpiDto? AssetTotal,
    long? NetResultDeltaVsSameMonthLastYearCents,
    DashboardDataSufficiencyState DataSufficiencyState,
    IReadOnlyList<DashboardDailyIncomeExpensePointDto> DailyIncomeVsExpense,
    IReadOnlyList<DashboardGroupStatePointDto> GroupStates,
    DashboardYtdSummaryDto YtdSummary,
    IReadOnlyList<DashboardCompactInsightRowDto> CompactInsights,
    IReadOnlyList<DashboardExpenseKindRankDto>? ExpenseKindRanking = null,
    IReadOnlyList<DashboardPinnedGroupOperationalResultDto>? PinnedGroups = null
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
