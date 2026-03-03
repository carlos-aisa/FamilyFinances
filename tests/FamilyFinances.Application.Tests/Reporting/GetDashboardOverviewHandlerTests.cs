using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Handlers;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class GetDashboardOverviewHandlerTests
{
    [Fact]
    public async Task HandleAsync_Maps_Overview_And_Enforces_AnomalyFirst_RowCap()
    {
        var asOf = new DateOnly(2026, 3, 15);
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var calculator = new Mock<IReportingInsightsCalculator>(MockBehavior.Strict);

        var core = BuildCore(asOf, hasPreviousMonthData: true, hasSameMonthLastYearData: false);
        repo.Setup(r => r.GetDashboardOverviewCoreAsync(asOf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(core);
        repo.Setup(r => r.GetInsightContributorTotalsAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new InsightContributorAggregateDto(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Groceries", 80_000)
            ]);
        repo.Setup(r => r.GetMonthlyInsightContributorTotalsAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new InsightMonthlyContributorAggregateDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Utilities", 2026, 3, 40_000)
            ]);

        calculator
            .Setup(c => c.BuildParetoInsights(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                ReportingInsightDimension.Group,
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<InsightContributorAggregateDto>>(),
                It.IsAny<IReadOnlyList<InsightContributorAggregateDto>>()))
            .Returns(new ReportingParetoInsightsDto(
                From: new DateOnly(2026, 3, 1),
                To: new DateOnly(2026, 3, 16),
                Dimension: ReportingInsightDimension.Group,
                Expense: new ParetoInsightSectionDto(
                    Nature: AccountNature.Expense,
                    TotalAmountCents: 300_000,
                    TopN: 8,
                    TopNAmountCents: 280_000,
                    TopNCoveragePercentage: 93.33m,
                    Contributors:
                    [
                        new ParetoContributorDto(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Rent", 120_000, 40m),
                        new ParetoContributorDto(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Groceries", 80_000, 26.66m),
                        new ParetoContributorDto(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Transport", 50_000, 16.66m),
                        new ParetoContributorDto(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Utilities", 30_000, 10m),
                        new ParetoContributorDto(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Health", 20_000, 6.66m)
                    ]),
                Income: new ParetoInsightSectionDto(
                    Nature: AccountNature.Income,
                    TotalAmountCents: 0,
                    TopN: 8,
                    TopNAmountCents: 0,
                    TopNCoveragePercentage: 0m,
                    Contributors: Array.Empty<ParetoContributorDto>())));

        calculator
            .Setup(c => c.BuildMonthlyAnomalyInsights(
                2026,
                3,
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<InsightMonthlyContributorAggregateDto>>()))
            .Returns(new ReportingAnomalyInsightsDto(
                Year: 2026,
                Month: 3,
                Nature: AccountNature.Expense,
                Dimension: ReportingInsightDimension.Group,
                RequiredHistoryMonths: 3,
                ThresholdRule: "mean + 2sigma",
                Contributors:
                [
                    new AnomalyContributorDto(
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        "Groceries",
                        80_000,
                        30_000,
                        50_000,
                        2.5m,
                        true,
                        false,
                        6,
                        "threshold exceeded"),
                    new AnomalyContributorDto(
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        "Utilities",
                        30_000,
                        10_000,
                        20_000,
                        null,
                        true,
                        true,
                        2,
                        "insufficient history")
                ]));

        var handler = new GetDashboardOverviewHandler(repo.Object, calculator.Object);

        var result = await handler.HandleAsync(new GetDashboardOverviewQuery(asOf), CancellationToken.None);

        result.DataSufficiencyState.Should().Be(DashboardDataSufficiencyState.Partial);
        result.Income.ValueCents.Should().Be(core.CurrentState.IncomeTotalCents);
        result.NetResult.ValueCents.Should().Be(core.CurrentState.PeriodNetResultCents);
        result.GroupStates.Should().HaveCount(2);

        result.CompactInsights.Should().HaveCount(6);
        result.CompactInsights.Take(2).Select(r => r.Kind).Should().OnlyContain(kind => kind == "anomaly");
        result.CompactInsights.Skip(2).Select(r => r.Kind).Should().OnlyContain(kind => kind == "top-expense");

        repo.VerifyAll();
        calculator.VerifyAll();
    }

    [Theory]
    [InlineData(true, true, DashboardDataSufficiencyState.Complete)]
    [InlineData(true, false, DashboardDataSufficiencyState.Partial)]
    [InlineData(false, false, DashboardDataSufficiencyState.InsufficientHistory)]
    public async Task HandleAsync_Resolves_DataSufficiency_From_Core_History_Flags(
        bool hasPreviousMonthData,
        bool hasSameMonthLastYearData,
        DashboardDataSufficiencyState expectedState)
    {
        var asOf = new DateOnly(2026, 3, 10);
        var repo = new Mock<IReportingReadRepository>(MockBehavior.Strict);
        var calculator = new Mock<IReportingInsightsCalculator>(MockBehavior.Strict);

        repo.Setup(r => r.GetDashboardOverviewCoreAsync(asOf, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCore(asOf, hasPreviousMonthData, hasSameMonthLastYearData));
        repo.Setup(r => r.GetInsightContributorTotalsAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightContributorAggregateDto>());
        repo.Setup(r => r.GetMonthlyInsightContributorTotalsAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<InsightMonthlyContributorAggregateDto>());

        calculator
            .Setup(c => c.BuildParetoInsights(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                ReportingInsightDimension.Group,
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<InsightContributorAggregateDto>>(),
                It.IsAny<IReadOnlyList<InsightContributorAggregateDto>>()))
            .Returns(new ReportingParetoInsightsDto(
                From: new DateOnly(2026, 3, 1),
                To: new DateOnly(2026, 3, 11),
                Dimension: ReportingInsightDimension.Group,
                Expense: new ParetoInsightSectionDto(AccountNature.Expense, 0, 8, 0, 0, Array.Empty<ParetoContributorDto>()),
                Income: new ParetoInsightSectionDto(AccountNature.Income, 0, 8, 0, 0, Array.Empty<ParetoContributorDto>())));

        calculator
            .Setup(c => c.BuildMonthlyAnomalyInsights(
                2026,
                3,
                AccountNature.Expense,
                ReportingInsightDimension.Group,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<InsightMonthlyContributorAggregateDto>>()))
            .Returns(new ReportingAnomalyInsightsDto(
                Year: 2026,
                Month: 3,
                Nature: AccountNature.Expense,
                Dimension: ReportingInsightDimension.Group,
                RequiredHistoryMonths: 3,
                ThresholdRule: "mean + 2sigma",
                Contributors: Array.Empty<AnomalyContributorDto>()));

        var handler = new GetDashboardOverviewHandler(repo.Object, calculator.Object);

        var result = await handler.HandleAsync(new GetDashboardOverviewQuery(asOf), CancellationToken.None);

        result.DataSufficiencyState.Should().Be(expectedState);
        repo.VerifyAll();
        calculator.VerifyAll();
    }

    private static DashboardOverviewCoreDto BuildCore(
        DateOnly asOf,
        bool hasPreviousMonthData,
        bool hasSameMonthLastYearData)
    {
        return new DashboardOverviewCoreDto(
            AsOf: asOf,
            SelectedMonthStart: new DateOnly(asOf.Year, asOf.Month, 1),
            SelectedMonthEnd: new DateOnly(asOf.Year, asOf.Month, DateTime.DaysInMonth(asOf.Year, asOf.Month)),
            PreviousMonthStart: new DateOnly(asOf.AddMonths(-1).Year, asOf.AddMonths(-1).Month, 1),
            PreviousMonthEnd: new DateOnly(asOf.AddMonths(-1).Year, asOf.AddMonths(-1).Month, DateTime.DaysInMonth(asOf.AddMonths(-1).Year, asOf.AddMonths(-1).Month)),
            CurrentState: new EconomicStateDto(
                AsOf: asOf,
                AssetsTotalCents: 900_000,
                LiabilitiesTotalCents: 200_000,
                NetWorthCents: 700_000,
                IncomeTotalCents: 300_000,
                ExpenseTotalCents: 120_000,
                PeriodNetResultCents: 180_000),
            PreviousState: new EconomicStateDto(
                AsOf: asOf.AddMonths(-1),
                AssetsTotalCents: 850_000,
                LiabilitiesTotalCents: 220_000,
                NetWorthCents: 630_000,
                IncomeTotalCents: 250_000,
                ExpenseTotalCents: 110_000,
                PeriodNetResultCents: 140_000),
            IncomeDailyPoints:
            [
                new MonthlyChartPointDto(1, asOf, 200_000),
                new MonthlyChartPointDto(2, asOf, 100_000)
            ],
            ExpenseDailyPoints:
            [
                new MonthlyChartPointDto(1, asOf, 50_000),
                new MonthlyChartPointDto(2, asOf, 70_000)
            ],
            MonthlyNetPoints:
            [
                new DashboardMonthlyNetPointDto(1, 100_000, 40_000, 60_000, 60_000),
                new DashboardMonthlyNetPointDto(2, 120_000, 50_000, 70_000, 130_000),
                new DashboardMonthlyNetPointDto(asOf.Month, 300_000, 120_000, 180_000, 310_000)
            ],
            GroupStates:
            [
                new DashboardGroupStatePointDto("group:1", "Household", 80_000, 10_000),
                new DashboardGroupStatePointDto("group:2", "Income", -120_000, -15_000)
            ],
            HasPreviousMonthData: hasPreviousMonthData,
            HasSameMonthLastYearData: hasSameMonthLastYearData,
            SameMonthLastYearNetCents: hasSameMonthLastYearData ? 160_000 : null);
    }
}
