using Bunit;
using FamilyFinances.Web.Components.Pages.Reports.Charts;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class AnnualLineChartTests : WebTestContext
{
    [Fact]
    public void Export_Image_Button_Triggers_Download_For_Visible_Chart()
    {
        var exportCall = JSInterop.SetupVoid("familyFinancesCharts.downloadChartImage", _ => true);

        var cut = RenderComponent<AnnualLineChart>(parameters => parameters
            .Add(p => p.Title, "Asset Total Evolution")
            .Add(p => p.Year, 2026)
            .Add(p => p.Series,
            [
                new AnnualChartSeries(
                    "end-balance",
                    "End Balance",
                    "#0d6efd",
                    [new AnnualChartPoint(1, 1_000_000)])
            ]));

        cut.Find("[data-testid='annual-line-chart-export-image']").Click();

        exportCall.Invocations.Should().ContainSingle();
    }
}
