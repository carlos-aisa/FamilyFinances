using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Features.Dashboard;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Dashboard;

public sealed class DashboardMonthlySummaryBuilderTests
{
    [Fact]
    public void Build_Returns_At_Most_Four_Useful_Insights()
    {
        var insights = DashboardMonthlySummaryBuilder.Build(CreateOverview());

        insights.Should().HaveCount(4);
        insights.Select(insight => insight.Kind).Should().BeEquivalentTo(
        [
            DashboardMonthlySummaryInsightKind.MonthlyResult,
            DashboardMonthlySummaryInsightKind.PreviousMonthComparison,
            DashboardMonthlySummaryInsightKind.TopExpenseKind,
            DashboardMonthlySummaryInsightKind.TopPinnedGroup
        ], options => options.WithStrictOrdering());
    }

    [Fact]
    public void Build_Omits_PreviousMonth_Comparison_When_History_Is_Not_Complete()
    {
        var overview = CreateOverview(dataSufficiencyState: DashboardDataSufficiencyState.Partial);

        var insights = DashboardMonthlySummaryBuilder.Build(overview);

        insights.Should().NotContain(insight => insight.Kind == DashboardMonthlySummaryInsightKind.PreviousMonthComparison);
    }

    [Fact]
    public void Build_Uses_Highest_NonOthers_Expense_Kind()
    {
        var overview = CreateOverview(expenseKindRanking:
        [
            new DashboardExpenseKindRankDto(Guid.NewGuid(), "Food", 25_000, 25m, false),
            new DashboardExpenseKindRankDto(Guid.NewGuid(), "Housing", 60_000, 60m, false),
            new DashboardExpenseKindRankDto(null, "Others", 15_000, 15m, true)
        ]);

        var insight = DashboardMonthlySummaryBuilder.Build(overview)
            .Single(item => item.Kind == DashboardMonthlySummaryInsightKind.TopExpenseKind);

        insight.Label.Should().Be("Housing");
        insight.AmountCents.Should().Be(60_000);
    }

    [Fact]
    public void Build_Uses_Pinned_Group_With_Greatest_Monthly_Impact()
    {
        var overview = CreateOverview(pinnedGroups:
        [
            new DashboardPinnedGroupOperationalResultDto(Guid.NewGuid(), "Home", -40_000, -100_000),
            new DashboardPinnedGroupOperationalResultDto(Guid.NewGuid(), "Leisure", 65_000, 80_000)
        ]);

        var insight = DashboardMonthlySummaryBuilder.Build(overview)
            .Single(item => item.Kind == DashboardMonthlySummaryInsightKind.TopPinnedGroup);

        insight.Label.Should().Be("Leisure");
        insight.AmountCents.Should().Be(65_000);
    }

    [Fact]
    public void Build_Omits_Kind_And_Pinned_Group_Insights_When_Those_Data_Sets_Are_Empty()
    {
        var overview = CreateOverview(
            expenseKindRanking: Array.Empty<DashboardExpenseKindRankDto>(),
            pinnedGroups: Array.Empty<DashboardPinnedGroupOperationalResultDto>());

        var insights = DashboardMonthlySummaryBuilder.Build(overview);

        insights.Should().NotContain(insight => insight.Kind == DashboardMonthlySummaryInsightKind.TopExpenseKind);
        insights.Should().NotContain(insight => insight.Kind == DashboardMonthlySummaryInsightKind.TopPinnedGroup);
        insights.Should().Contain(insight => insight.Kind == DashboardMonthlySummaryInsightKind.AnnualAccumulation);
    }

    private static DashboardOverviewDto CreateOverview(
        DashboardDataSufficiencyState dataSufficiencyState = DashboardDataSufficiencyState.Complete,
        IReadOnlyList<DashboardExpenseKindRankDto>? expenseKindRanking = null,
        IReadOnlyList<DashboardPinnedGroupOperationalResultDto>? pinnedGroups = null)
    {
        return new DashboardOverviewDto(
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            new DashboardKpiDto(250_000, 10_000),
            new DashboardKpiDto(-90_000, 5_000),
            new DashboardKpiDto(160_000, 20_000),
            new DashboardKpiDto(1_200_000, 30_000),
            new DashboardKpiDto(1_500_000, 40_000),
            null,
            dataSufficiencyState,
            Array.Empty<DashboardDailyIncomeExpensePointDto>(),
            Array.Empty<DashboardGroupStatePointDto>(),
            new DashboardYtdSummaryDto(320_000, Array.Empty<DashboardMonthlyNetPointDto>()),
            Array.Empty<DashboardCompactInsightRowDto>(),
            expenseKindRanking ??
            [
                new DashboardExpenseKindRankDto(Guid.NewGuid(), "Food", 50_000, 50m, false)
            ],
            pinnedGroups ??
            [
                new DashboardPinnedGroupOperationalResultDto(Guid.NewGuid(), "Home", -40_000, -100_000)
            ]);
    }
}
