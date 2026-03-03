using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetDashboardOverviewHandler
{
    private const int ParetoTopN = 8;

    private readonly IReportingReadRepository _repo;
    private readonly IReportingInsightsCalculator _insightsCalculator;

    public GetDashboardOverviewHandler(
        IReportingReadRepository repo,
        IReportingInsightsCalculator insightsCalculator)
    {
        _repo = repo;
        _insightsCalculator = insightsCalculator;
    }

    public async Task<DashboardOverviewDto> HandleAsync(GetDashboardOverviewQuery query, CancellationToken ct)
    {
        var core = await _repo.GetDashboardOverviewCoreAsync(query.AsOf, ct);

        var income = new DashboardKpiDto(
            ValueCents: core.CurrentState.IncomeTotalCents,
            DeltaVsPreviousMonthCents: core.CurrentState.IncomeTotalCents - core.PreviousState.IncomeTotalCents);

        var expense = new DashboardKpiDto(
            ValueCents: core.CurrentState.ExpenseTotalCents,
            DeltaVsPreviousMonthCents: core.CurrentState.ExpenseTotalCents - core.PreviousState.ExpenseTotalCents);

        var netResult = new DashboardKpiDto(
            ValueCents: core.CurrentState.PeriodNetResultCents,
            DeltaVsPreviousMonthCents: core.CurrentState.PeriodNetResultCents - core.PreviousState.PeriodNetResultCents);

        var netWorth = new DashboardKpiDto(
            ValueCents: core.CurrentState.NetWorthCents,
            DeltaVsPreviousMonthCents: core.CurrentState.NetWorthCents - core.PreviousState.NetWorthCents);

        var dailyIncomeVsExpense = BuildDailyIncomeVsExpense(core.IncomeDailyPoints, core.ExpenseDailyPoints);
        var compactInsights = await BuildExpenseCompositionRowsAsync(core.AsOf, ct);

        long? sameMonthDelta = core.SameMonthLastYearNetCents is null
            ? null
            : core.CurrentState.PeriodNetResultCents - core.SameMonthLastYearNetCents.Value;

        return new DashboardOverviewDto(
            AsOf: core.AsOf,
            SelectedMonthStart: core.SelectedMonthStart,
            SelectedMonthEnd: core.SelectedMonthEnd,
            PreviousMonthStart: core.PreviousMonthStart,
            PreviousMonthEnd: core.PreviousMonthEnd,
            Income: income,
            Expense: expense,
            NetResult: netResult,
            NetWorth: netWorth,
            NetResultDeltaVsSameMonthLastYearCents: sameMonthDelta,
            DataSufficiencyState: ResolveDataSufficiency(core),
            DailyIncomeVsExpense: dailyIncomeVsExpense,
            GroupStates: core.GroupStates,
            YtdSummary: new DashboardYtdSummaryDto(
                AccumulatedNetCents: core.MonthlyNetPoints.LastOrDefault()?.AccumulatedNetCents ?? 0L,
                MonthlyNetPoints: core.MonthlyNetPoints),
            CompactInsights: compactInsights
        );
    }

    private static DashboardDataSufficiencyState ResolveDataSufficiency(DashboardOverviewCoreDto core)
    {
        if (core.HasPreviousMonthData && core.HasSameMonthLastYearData)
            return DashboardDataSufficiencyState.Complete;

        if (core.HasPreviousMonthData || core.HasSameMonthLastYearData)
            return DashboardDataSufficiencyState.Partial;

        return DashboardDataSufficiencyState.InsufficientHistory;
    }

    private static IReadOnlyList<DashboardDailyIncomeExpensePointDto> BuildDailyIncomeVsExpense(
        IReadOnlyList<MonthlyChartPointDto> incomePoints,
        IReadOnlyList<MonthlyChartPointDto> expensePoints)
    {
        var incomeByDay = incomePoints.ToDictionary(p => p.Day, p => p.EndBalanceCents);
        var expenseByDay = expensePoints.ToDictionary(p => p.Day, p => p.EndBalanceCents);
        var days = incomeByDay.Keys
            .Concat(expenseByDay.Keys)
            .Distinct()
            .OrderBy(day => day)
            .ToList();

        return days
            .Select(day =>
            {
                var income = incomeByDay.GetValueOrDefault(day, 0L);
                var expense = expenseByDay.GetValueOrDefault(day, 0L);
                return new DashboardDailyIncomeExpensePointDto(
                    Day: day,
                    IncomeCents: income,
                    ExpenseCents: expense,
                    NetCents: income - expense);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<DashboardCompactInsightRowDto>> BuildExpenseCompositionRowsAsync(DateOnly asOf, CancellationToken ct)
    {
        var monthStart = new DateOnly(asOf.Year, asOf.Month, 1);
        var monthEndExclusive = asOf.AddDays(1);

        var expenseContributors = await _repo.GetInsightContributorTotalsAsync(
            monthStart,
            monthEndExclusive,
            AccountNature.Expense,
            ReportingInsightDimension.Group,
            accountId: null,
            payeeId: null,
            ct);

        var pareto = _insightsCalculator.BuildParetoInsights(
            monthStart,
            monthEndExclusive,
            ReportingInsightDimension.Group,
            ParetoTopN,
            expenseContributors,
            incomeContributors: Array.Empty<InsightContributorAggregateDto>());

        var topContributors = pareto.Expense.Contributors
            .OrderByDescending(x => x.AmountCents)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var totalExpenseAmount = Math.Abs(pareto.Expense.TotalAmountCents);
        if (totalExpenseAmount <= 0L || topContributors.Count == 0)
            return Array.Empty<DashboardCompactInsightRowDto>();

        var rows = topContributors
            .Select(contributor => new DashboardCompactInsightRowDto(
                RowKey: contributor.EntityId is null
                    ? $"top:{contributor.DisplayName}"
                    : $"top:{contributor.EntityId.Value:D}",
                Kind: "top-expense",
                Label: contributor.DisplayName,
                AmountCents: Math.Abs(contributor.AmountCents),
                Percentage: contributor.ContributionPercentage,
                StatusCode: "top-contributor"))
            .ToList();

        var topAmount = rows.Sum(row => row.AmountCents);
        var othersAmount = Math.Max(0L, totalExpenseAmount - topAmount);
        if (othersAmount > 0L)
        {
            rows.Add(new DashboardCompactInsightRowDto(
                RowKey: "top:others",
                Kind: "top-expense",
                Label: "Others",
                AmountCents: othersAmount,
                Percentage: Math.Round((othersAmount * 100m) / totalExpenseAmount, 2, MidpointRounding.AwayFromZero),
                StatusCode: "others"));
        }

        return rows;
    }
}
