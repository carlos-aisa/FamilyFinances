using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Web.Features.Dashboard;

public enum DashboardMonthlySummaryInsightKind
{
    MonthlyResult,
    PreviousMonthComparison,
    TopExpenseKind,
    TopPinnedGroup,
    AnnualAccumulation,
    NetWorth
}

public sealed record DashboardMonthlySummaryInsight(
    DashboardMonthlySummaryInsightKind Kind,
    string? Label,
    long AmountCents);

public static class DashboardMonthlySummaryBuilder
{
    private const int MaximumInsightCount = 4;

    public static IReadOnlyList<DashboardMonthlySummaryInsight> Build(DashboardOverviewDto overview)
    {
        ArgumentNullException.ThrowIfNull(overview);

        var insights = new List<DashboardMonthlySummaryInsight>(MaximumInsightCount)
        {
            new(DashboardMonthlySummaryInsightKind.MonthlyResult, null, overview.NetResult.ValueCents)
        };

        if (overview.DataSufficiencyState == DashboardDataSufficiencyState.Complete)
        {
            insights.Add(new(
                DashboardMonthlySummaryInsightKind.PreviousMonthComparison,
                null,
                overview.NetResult.DeltaVsPreviousMonthCents));
        }

        var topExpenseKind = overview.ExpenseKindRanking?
            .Where(row => !row.IsOthers && row.AmountCents > 0L)
            .OrderByDescending(row => row.AmountCents)
            .ThenBy(row => row.Label, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (topExpenseKind is not null && insights.Count < MaximumInsightCount)
        {
            insights.Add(new(
                DashboardMonthlySummaryInsightKind.TopExpenseKind,
                topExpenseKind.Label,
                topExpenseKind.AmountCents));
        }

        var topPinnedGroup = overview.PinnedGroups?
            .Where(group => group.MonthOperationalResultCents != 0L)
            .OrderByDescending(group => Math.Abs(group.MonthOperationalResultCents))
            .ThenBy(group => group.GroupName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (topPinnedGroup is not null && insights.Count < MaximumInsightCount)
        {
            insights.Add(new(
                DashboardMonthlySummaryInsightKind.TopPinnedGroup,
                topPinnedGroup.GroupName,
                topPinnedGroup.MonthOperationalResultCents));
        }

        if (insights.Count <= 2)
        {
            insights.Add(new(
                DashboardMonthlySummaryInsightKind.AnnualAccumulation,
                null,
                overview.YtdSummary.AccumulatedNetCents));
        }

        if (insights.Count <= 2)
        {
            insights.Add(new(
                DashboardMonthlySummaryInsightKind.NetWorth,
                null,
                overview.NetWorth.ValueCents));
        }

        return insights;
    }
}
