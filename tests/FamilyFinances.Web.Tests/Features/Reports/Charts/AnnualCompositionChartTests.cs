using System.Text.Json;
using Bunit;
using FamilyFinances.Web.Components.Pages.Reports.Charts;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class AnnualCompositionChartTests : WebTestContext
{
    [Fact]
    public void MoreThanTenSlices_AggregatesTailIntoOthers_AndRendersSideLegend()
    {
        var renderCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualCompositionChart", _ => true);

        var slices = Enumerable.Range(1, 12)
            .Select(index => new AnnualCompositionSlice(
                Key: $"slice-{index}",
                Label: $"Slice {index}",
                RawValueCents: (13 - index) * 100,
                Percentage: 0m,
                ColorHex: AnnualChartPalette.Resolve(index - 1)))
            .ToList();

        var cut = RenderComponent<AnnualCompositionChart>(parameters => parameters
            .Add(p => p.Title, "Expense Composition")
            .Add(p => p.Year, 2026)
            .Add(p => p.UseSideLegend, true)
            .Add(p => p.LegendMaxItems, 10)
            .Add(p => p.MaxSlices, 10)
            .Add(p => p.Slices, slices));

        cut.Find("[data-testid='annual-composition-chart']")
            .GetAttribute("data-total-percentage")
            .Should().NotBeNullOrWhiteSpace();

        var legendRows = cut.FindAll("[data-testid='annual-composition-chart-legend'] .composition-slice-row");
        legendRows.Should().HaveCount(10);
        legendRows.Select(row => row.TextContent).Should().Contain(text => text.Contains("Others", StringComparison.OrdinalIgnoreCase));
        legendRows.Select(row => row.TextContent).Should().NotContain(text => text.Contains("Slice 10", StringComparison.OrdinalIgnoreCase));

        renderCall.Invocations.Should().ContainSingle();
        var payloadJson = JsonSerializer.Serialize(renderCall.Invocations.Single().Arguments[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var labels = payload.RootElement.GetProperty("labels").EnumerateArray().Select(label => label.GetString()).ToList();

        labels.Should().HaveCount(10);
        labels.Should().Contain("Others");
        labels.Should().NotContain("Slice 10");
    }
}
