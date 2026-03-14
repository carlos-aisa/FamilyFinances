using System.Text.Json;
using Bunit;
using FamilyFinances.Web.Components.Pages.Reports.Charts;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class MonthlyLineChartTests : WebTestContext
{
    [Fact]
    public void FullMonthRange_Builds_AllDays_Extends_Line_And_Sets_CutoffMarker()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Month-focused income vs expense")
            .Add(p => p.Mode, EvolutionChartMode.DailyInMonth)
            .Add(p => p.Year, 2026)
            .Add(p => p.Month, 3)
            .Add(p => p.ShowFullRange, true)
            .Add(p => p.DailySeries,
            [
                new MonthlyChartSeries(
                    "income",
                    "Income",
                    "#2dd67d",
                    [
                        new MonthlyChartPoint(1, 100m),
                        new MonthlyChartPoint(3, 300m)
                    ],
                    BaselineValue: 100m)
            ]));

        renderCall.Invocations.Should().ContainSingle();
        var invocation = renderCall.Invocations.Single();
        invocation.Arguments.Should().HaveCount(2);

        var payloadJson = JsonSerializer.Serialize(invocation.Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var root = payload.RootElement;

        var labels = root.GetProperty("labels");
        labels.GetArrayLength().Should().Be(31);
        labels[0].GetString().Should().Be("1");
        labels[30].GetString().Should().Be("31");

        root.GetProperty("markerDay").GetInt32().Should().Be(3);
        root.GetProperty("totalDays").GetInt32().Should().Be(31);

        var values = root.GetProperty("datasets")[0].GetProperty("values");
        values.GetArrayLength().Should().Be(31);
        values[0].GetDouble().Should().BeApproximately(0d, 0.0001d);
        values[1].GetDouble().Should().BeApproximately(0d, 0.0001d);
        values[2].GetDouble().Should().BeApproximately(2d, 0.0001d);
        values[30].GetDouble().Should().BeApproximately(2d, 0.0001d);
    }

    [Fact]
    public void Payload_Uses_Semantic_Color_For_Income_Series()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Income")
            .Add(p => p.Mode, EvolutionChartMode.DailyInMonth)
            .Add(p => p.Year, 2026)
            .Add(p => p.Month, 3)
            .Add(p => p.DailySeries,
            [
                new MonthlyChartSeries(
                    "income",
                    "Income",
                    ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Income),
                    [new MonthlyChartPoint(1, 100m)],
                    BaselineValue: 0m)
            ]));

        var payloadJson = JsonSerializer.Serialize(renderCall.Invocations.Single().Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var colorHex = payload.RootElement
            .GetProperty("datasets")[0]
            .GetProperty("colorHex")
            .GetString();

        colorHex.Should().Be(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Income));
    }

    [Fact]
    public void DailyNormalization_Uses_OpeningBalance_And_Preserves_FirstDayMovement()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Month-focused balance")
            .Add(p => p.Mode, EvolutionChartMode.DailyInMonth)
            .Add(p => p.Year, 2026)
            .Add(p => p.Month, 3)
            .Add(p => p.ShowFullRange, true)
            .Add(p => p.DailySeries,
            [
                new MonthlyChartSeries(
                    "balance",
                    "Balance",
                    "#4fa4ff",
                    [
                        new MonthlyChartPoint(1, 100m),
                        new MonthlyChartPoint(2, 130m)
                    ],
                    BaselineValue: 90m)
            ]));

        var payloadJson = JsonSerializer.Serialize(renderCall.Invocations.Single().Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var values = payload.RootElement.GetProperty("datasets")[0].GetProperty("values");

        values[0].GetDouble().Should().BeApproximately(0.1d, 0.0001d);
        values[1].GetDouble().Should().BeApproximately(0.4d, 0.0001d);
    }
}
