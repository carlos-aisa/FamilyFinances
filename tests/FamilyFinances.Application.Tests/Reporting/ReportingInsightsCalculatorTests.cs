using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class ReportingInsightsCalculatorTests
{
    private readonly ReportingInsightsCalculator _sut = new();

    [Fact]
    public void BuildParetoInsights_Orders_By_Contribution_And_Computes_Percentages()
    {
        var result = _sut.BuildParetoInsights(
            from: new DateOnly(2026, 2, 1),
            to: new DateOnly(2026, 3, 1),
            dimension: ReportingInsightDimension.Group,
            topN: 2,
            expenseContributors:
            [
                new InsightContributorAggregateDto(Guid.NewGuid(), "Food", 50_000),
                new InsightContributorAggregateDto(Guid.NewGuid(), "Transport", 30_000),
                new InsightContributorAggregateDto(Guid.NewGuid(), "Rent", 20_000)
            ],
            incomeContributors:
            [
                new InsightContributorAggregateDto(Guid.NewGuid(), "Salary", 120_000)
            ]);

        result.Expense.TotalAmountCents.Should().Be(100_000);
        result.Expense.TopNAmountCents.Should().Be(80_000);
        result.Expense.TopNCoveragePercentage.Should().Be(80m);
        result.Expense.Contributors.Select(x => x.DisplayName).Should().Equal("Food", "Transport");
        result.Expense.Contributors.Select(x => x.ContributionPercentage).Should().Equal(50m, 30m);
    }

    [Fact]
    public void BuildMonthlyAnomalyInsights_Flags_Contributor_When_Above_Threshold()
    {
        var groupId = Guid.NewGuid();

        var result = _sut.BuildMonthlyAnomalyInsights(
            year: 2026,
            month: 2,
            nature: AccountNature.Expense,
            dimension: ReportingInsightDimension.Group,
            lookbackMonths: 6,
            requiredHistoryMonths: 3,
            monthlyContributors:
            [
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2025, 8, 10_000),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2025, 9, 10_500),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2025, 10, 9_500),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2025, 11, 10_000),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2025, 12, 9_800),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2026, 1, 10_200),
                new InsightMonthlyContributorAggregateDto(groupId, "Housing", 2026, 2, 20_000)
            ]);

        var contributor = result.Contributors.Should().ContainSingle().Subject;
        contributor.IsInsufficientHistory.Should().BeFalse();
        contributor.IsAnomaly.Should().BeTrue();
        contributor.CurrentAmountCents.Should().Be(20_000);
        contributor.ThresholdAmountCents.Should().BeLessThan(20_000);
    }

    [Fact]
    public void BuildMonthlyAnomalyInsights_Returns_InsufficientHistory_State_Without_Flag()
    {
        var payeeId = Guid.NewGuid();

        var result = _sut.BuildMonthlyAnomalyInsights(
            year: 2026,
            month: 2,
            nature: AccountNature.Expense,
            dimension: ReportingInsightDimension.Payee,
            lookbackMonths: 6,
            requiredHistoryMonths: 3,
            monthlyContributors:
            [
                new InsightMonthlyContributorAggregateDto(payeeId, "Mercadona", 2026, 1, 8_000),
                new InsightMonthlyContributorAggregateDto(payeeId, "Mercadona", 2026, 2, 40_000)
            ]);

        var contributor = result.Contributors.Should().ContainSingle().Subject;
        contributor.IsInsufficientHistory.Should().BeTrue();
        contributor.IsAnomaly.Should().BeFalse();
        contributor.Explanation.Should().Contain("Insufficient history");
    }
}
