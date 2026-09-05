using Bunit;
using FamilyFinances.Web.Components.Pages.Reports.Charts;
using FamilyFinances.Web.Features.Reports;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Reports.Charts;

public sealed class AnnualEvolutionListTests : WebTestContext
{
    [Fact]
    public void AccountGroupLayout_RendersSemanticAmountsAndDrillDownLink()
    {
        var groupId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var cut = RenderComponent<AnnualEvolutionList>(parameters => parameters
            .Add(component => component.Title, "Groups")
            .Add(component => component.Year, 2026)
            .Add(component => component.Month, 3)
            .Add(component => component.Layout, AnnualEvolutionListLayout.AccountGroup)
            .Add(component => component.EnableDetailNavigation, true)
            .Add(component => component.Items,
            [
                new AnnualEvolutionListItem(groupId, "group:food", "Food", 10_000, -2_500, -5_000, "text-danger")
            ]));

        var row = cut.Find("tbody tr");
        row.TextContent.Should().Contain(MoneyFormatter.FormatCentsWithSign(-2_500));
        row.QuerySelector("td.text-danger").Should().NotBeNull();
        row.QuerySelector("a")!.GetAttribute("href").Should()
            .Be($"/reports/account-group-totals?groupId={groupId}&year=2026&month=03");
        row.Click();
        Services.GetRequiredService<NavigationManager>().Uri.Should()
            .Be($"http://localhost/reports/account-group-totals?groupId={groupId}&year=2026&month=03");
        cut.Find("[data-testid='annual-evolution-list-export-csv']").TextContent.Should().Contain("Export CSV");
    }

}
