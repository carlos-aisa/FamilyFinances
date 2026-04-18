using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class AnnualChartDatasetAdapterTests
{
    [Fact]
    public void BuildEndBalanceSeries_ReturnsEmpty_WhenReportHasNoSeries()
    {
        var report = new MonthlyEvolutionReportDto(2026, MonthlyEvolutionScope.Accounts, []);

        var result = AnnualChartDatasetAdapter.BuildEndBalanceSeries(report);

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildEndBalanceSeries_SumsBalances_ByMonthAcrossSeries()
    {
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("checking", "Checking", points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 10_000, 0, 0),
                    new MonthlyEvolutionPointDto(2, new DateOnly(2026, 2, 28), 15_000, 5_000, 5_000)
                ]),
                BuildSeries("savings", "Savings", points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 40_000, 0, 0),
                    new MonthlyEvolutionPointDto(2, new DateOnly(2026, 2, 28), 41_000, 1_000, 1_000)
                ])
            ]);

        var result = AnnualChartDatasetAdapter.BuildEndBalanceSeries(report);

        result.Should().ContainSingle();
        result[0].Points.Should().BeEquivalentTo(
            [
                new AnnualChartPoint(1, 50_000),
                new AnnualChartPoint(2, 56_000)
            ]);
    }

    [Fact]
    public void BuildSeriesEvolution_AppliesIncludedKeysAndHonorsMinimumTakeOfOne()
    {
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("z-key", "Zeta", points:
                [
                    new MonthlyEvolutionPointDto(2, new DateOnly(2026, 2, 28), 20_000, 3_000, 3_000),
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 17_000, 0, 0)
                ]),
                BuildSeries("a-key", "Alpha", points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 10_000, 0, 0)
                ])
            ]);

        var result = AnnualChartDatasetAdapter.BuildSeriesEvolution(
            report,
            maxSeries: 0,
            includedSeriesKeys: new HashSet<string> { "z-key" });

        result.Should().ContainSingle();
        result[0].Key.Should().Be("z-key");
        result[0].Points.Select(point => point.Month).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public void BuildSeriesMonthlyDelta_UsesDeltaValues()
    {
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("checking", "Checking", points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 10_000, 0, 0),
                    new MonthlyEvolutionPointDto(2, new DateOnly(2026, 2, 28), 9_500, -500, -500)
                ])
            ]);

        var result = AnnualChartDatasetAdapter.BuildSeriesMonthlyDelta(report);

        result.Should().ContainSingle();
        result[0].Points.Should().BeEquivalentTo(
            [
                new AnnualChartPoint(1, 0),
                new AnnualChartPoint(2, -500)
            ]);
    }

    [Fact]
    public void BuildCompositionByNature_ReturnsOnlyMatchingNature_WithPercentages()
    {
        var expenseId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("groceries", "Groceries", expenseId, points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), -20_000, 0, 0)
                ]),
                BuildSeries("wallet", "Wallet", assetId, points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 5_000, 0, 0)
                ])
            ]);

        var natureMap = new Dictionary<Guid, AccountNature>
        {
            [expenseId] = AccountNature.Expense,
            [assetId] = AccountNature.Asset
        };

        var result = AnnualChartDatasetAdapter.BuildCompositionByNature(report, natureMap, AccountNature.Expense);

        result.Should().ContainSingle();
        result[0].Key.Should().Be("groceries");
        result[0].RawValueCents.Should().Be(20_000);
        result[0].Percentage.Should().Be(100m);
    }

    [Fact]
    public void BuildCompositionByNatureAtMonth_UsesLatestPointAtOrBeforeSelectedMonth()
    {
        var expenseId = Guid.NewGuid();
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("housing", "Housing", expenseId, points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), -80_000, 0, 0),
                    new MonthlyEvolutionPointDto(3, new DateOnly(2026, 3, 31), -60_000, 20_000, 20_000)
                ])
            ]);

        var natureMap = new Dictionary<Guid, AccountNature>
        {
            [expenseId] = AccountNature.Expense
        };

        var result = AnnualChartDatasetAdapter.BuildCompositionByNatureAtMonth(report, natureMap, AccountNature.Expense, month: 2);

        result.Should().ContainSingle();
        result[0].RawValueCents.Should().Be(80_000);
    }

    [Fact]
    public void BuildCompositionFromSeriesAtMonth_ReturnsEmpty_WhenPredicateDoesNotMatch()
    {
        var report = new MonthlyEvolutionReportDto(
            2026,
            MonthlyEvolutionScope.Accounts,
            [
                BuildSeries("checking", "Checking", points:
                [
                    new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 10_000, 0, 0)
                ])
            ]);

        var result = AnnualChartDatasetAdapter.BuildCompositionFromSeriesAtMonth(report, series => series.SeriesKey == "missing", month: 1);

        result.Should().BeEmpty();
    }

    private static MonthlyEvolutionSeriesDto BuildSeries(
        string key,
        string name,
        Guid? entityId = null,
        IReadOnlyList<MonthlyEvolutionPointDto>? points = null)
    {
        return new MonthlyEvolutionSeriesDto(
            key,
            name,
            entityId,
            "account",
            points ?? []);
    }
}
