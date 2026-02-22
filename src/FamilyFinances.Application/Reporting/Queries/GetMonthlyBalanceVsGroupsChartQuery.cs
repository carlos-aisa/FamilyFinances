namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlyBalanceVsGroupsChartQuery(
    int Year,
    int Month
);
