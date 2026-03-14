using Bunit;
using System.Text.Json;
using FamilyFinances.Web.Components.Pages.Reports.Charts;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class AnnualLineChartTests : WebTestContext
{
    [Fact]
    public void CurrentYear_Payload_Includes_MarkerMonth_And_Nulls_Future_Months()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Asset Total Evolution")
            .Add(p => p.Mode, EvolutionChartMode.MonthlyInYear)
            .Add(p => p.TestId, "annual-line-chart")
            .Add(p => p.Year, 2026)
            .Add(p => p.DataUntilMonth, 3)
            .Add(p => p.AnnualSeries,
            [
                new AnnualChartSeries(
                    "end-balance",
                    "End Balance",
                    "#0d6efd",
                    [
                        new AnnualChartPoint(1, 100_00m),
                        new AnnualChartPoint(2, 120_00m),
                        new AnnualChartPoint(3, 150_00m),
                        new AnnualChartPoint(4, 170_00m)
                    ])
            ]));

        renderCall.Invocations.Should().ContainSingle();
        var invocation = renderCall.Invocations.Single();
        invocation.Arguments.Should().HaveCount(2);

        var payloadJson = JsonSerializer.Serialize(invocation.Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var root = payload.RootElement;

        root.GetProperty("markerMonth").GetInt32().Should().Be(3);
        root.GetProperty("totalMonths").GetInt32().Should().Be(12);

        var values = root.GetProperty("datasets")[0].GetProperty("values");
        values.GetArrayLength().Should().Be(12);
        values[0].GetDouble().Should().BeApproximately(100d, 0.0001d);
        values[1].GetDouble().Should().BeApproximately(120d, 0.0001d);
        values[2].GetDouble().Should().BeApproximately(150d, 0.0001d);
        values[3].ValueKind.Should().Be(JsonValueKind.Null);
        values[11].ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void CarryForwardAfterMarker_Fills_Future_Months_With_Last_Known_Value()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Account Evolution")
            .Add(p => p.Mode, EvolutionChartMode.MonthlyInYear)
            .Add(p => p.TestId, "annual-line-chart")
            .Add(p => p.Year, 2026)
            .Add(p => p.DataUntilMonth, 3)
            .Add(p => p.CarryForwardAfterMarker, true)
            .Add(p => p.AnnualSeries,
            [
                new AnnualChartSeries(
                    "account-a",
                    "Account A",
                    "#0d6efd",
                    [
                        new AnnualChartPoint(1, 100_00m),
                        new AnnualChartPoint(2, 130_00m),
                        new AnnualChartPoint(3, 150_00m)
                    ])
            ]));

        var payloadJson = JsonSerializer.Serialize(renderCall.Invocations.Single().Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var values = payload.RootElement.GetProperty("datasets")[0].GetProperty("values");

        values[0].GetDouble().Should().BeApproximately(100d, 0.0001d);
        values[1].GetDouble().Should().BeApproximately(130d, 0.0001d);
        values[2].GetDouble().Should().BeApproximately(150d, 0.0001d);
        values[3].GetDouble().Should().BeApproximately(150d, 0.0001d);
        values[11].GetDouble().Should().BeApproximately(150d, 0.0001d);
    }

    [Fact]
    public void Export_Image_Button_Triggers_Download_For_Visible_Chart()
    {
        var exportCall = JSInterop.SetupVoid("familyFinancesCharts.downloadChartImage", _ => true);

        var cut = RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Asset Total Evolution")
            .Add(p => p.Mode, EvolutionChartMode.MonthlyInYear)
            .Add(p => p.TestId, "annual-line-chart")
            .Add(p => p.Year, 2026)
            .Add(p => p.AnnualSeries,
            [
                new AnnualChartSeries(
                    "end-balance",
                    "End Balance",
                    "#0d6efd",
                    [new AnnualChartPoint(1, 1_000_000)])
            ]));

        cut.Markup.Should().Contain("ff-chart-panel");
        cut.Markup.Should().Contain("ff-chart-export-button");
        cut.Find("[data-testid='annual-line-chart-export-image']").Click();

        exportCall.Invocations.Should().ContainSingle();
    }

    [Fact]
    public void Payload_Preserves_Semantic_Color_For_Balance_Series()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);

        RenderComponent<EvolutionChart>(parameters => parameters
            .Add(p => p.Title, "Balance")
            .Add(p => p.Mode, EvolutionChartMode.MonthlyInYear)
            .Add(p => p.TestId, "annual-line-chart")
            .Add(p => p.Year, 2026)
            .Add(p => p.AnnualSeries,
            [
                new AnnualChartSeries(
                    "end-balance",
                    "End Balance",
                    ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Balance),
                    [new AnnualChartPoint(1, 1_000_000)])
            ]));

        var payloadJson = JsonSerializer.Serialize(renderCall.Invocations.Single().Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var colorHex = payload.RootElement
            .GetProperty("datasets")[0]
            .GetProperty("colorHex")
            .GetString();

        colorHex.Should().Be(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Balance));
    }
}
