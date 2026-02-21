using FamilyFinances.Application.Reporting.Semantics;
using FluentAssertions;

namespace FamilyFinances.Application.Tests.Reporting;

public sealed class ReportingMetricSemanticsTests
{
    [Fact]
    public void Definitions_Contain_Required_Canonical_Metrics_With_Expected_Families()
    {
        var definitions = ReportingMetricSemantics.Definitions;

        definitions.Should().ContainKey(ReportingMetricKey.AssetBalance);
        definitions[ReportingMetricKey.AssetBalance].CanonicalName.Should().Be("Asset Balance");
        definitions[ReportingMetricKey.AssetBalance].Family.Should().Be(ReportingMetricFamily.Stock);

        definitions.Should().ContainKey(ReportingMetricKey.LiabilityBalance);
        definitions[ReportingMetricKey.LiabilityBalance].CanonicalName.Should().Be("Liability Balance");
        definitions[ReportingMetricKey.LiabilityBalance].Family.Should().Be(ReportingMetricFamily.Stock);

        definitions.Should().ContainKey(ReportingMetricKey.NetWorth);
        definitions[ReportingMetricKey.NetWorth].CanonicalName.Should().Be("Net Worth");
        definitions[ReportingMetricKey.NetWorth].Family.Should().Be(ReportingMetricFamily.Stock);

        definitions.Should().ContainKey(ReportingMetricKey.Income);
        definitions[ReportingMetricKey.Income].CanonicalName.Should().Be("Income");
        definitions[ReportingMetricKey.Income].Family.Should().Be(ReportingMetricFamily.Flow);

        definitions.Should().ContainKey(ReportingMetricKey.Expense);
        definitions[ReportingMetricKey.Expense].CanonicalName.Should().Be("Expense");
        definitions[ReportingMetricKey.Expense].Family.Should().Be(ReportingMetricFamily.Flow);

        definitions.Should().ContainKey(ReportingMetricKey.PeriodNetResult);
        definitions[ReportingMetricKey.PeriodNetResult].CanonicalName.Should().Be("Period Net Result");
        definitions[ReportingMetricKey.PeriodNetResult].Family.Should().Be(ReportingMetricFamily.Flow);
    }

    [Theory]
    [InlineData("monthly-summary-income", ReportingMetricKey.Income)]
    [InlineData("monthly-summary-expense", ReportingMetricKey.Expense)]
    [InlineData("monthly-summary-period-net-result", ReportingMetricKey.PeriodNetResult)]
    [InlineData("monthly-evolution-latest-asset-end-balance", ReportingMetricKey.AssetBalance)]
    [InlineData("monthly-evolution-latest-asset-delta-prev-month", ReportingMetricKey.DeltaVsPreviousMonth)]
    [InlineData("monthly-evolution-latest-asset-delta-year-start", ReportingMetricKey.DeltaVsYearStart)]
    public void ResolveMetric_Returns_Expected_Metric_Key_For_Kpi_Id(string kpiId, ReportingMetricKey expectedKey)
    {
        var definition = ReportingMetricSemantics.ResolveMetric(kpiId);
        definition.Key.Should().Be(expectedKey);
    }

    [Fact]
    public void ResolveMetric_Throws_For_Unknown_Kpi_Id()
    {
        var act = () => ReportingMetricSemantics.ResolveMetric("unknown-kpi-id");

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*unknown-kpi-id*");
    }
}
