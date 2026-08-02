using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class ReportsIndexPageTests : WebTestContext
{
    [Fact]
    public void Authorized_User_Sees_Deterministic_Analytical_Families_And_Card_Order()
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();

        var families = cut.FindAll("[data-testid='reports-index-family']");
        families.Should().HaveCount(3);
        families.Select(family => family.TextContent).Should().SatisfyRespectively(
            financialSnapshot => financialSnapshot.Should().Contain("Financial Snapshot"),
            periodFlowAnalysis => periodFlowAnalysis.Should().Contain("Period Flow Analysis"),
            accountStructureAnalysis => accountStructureAnalysis.Should().Contain("Account Structure Analysis"));

        families[0].QuerySelectorAll(".report-card")
            .Select(card => card.GetAttribute("data-testid"))
            .Should().Equal(
                "reports-index-card-economic-state");
        families[1].QuerySelectorAll(".report-card")
            .Select(card => card.GetAttribute("data-testid"))
            .Should().Equal(
                "reports-index-card-monthly-summary",
                "reports-index-card-category-totals");
        families[2].QuerySelectorAll(".report-card")
            .Select(card => card.GetAttribute("data-testid"))
            .Should().Equal(
                "reports-index-card-account-totals",
                "reports-index-card-account-group-totals");
    }

    [Theory]
    [InlineData("reports-index-card-economic-state", "/reports/economic-state")]
    [InlineData("reports-index-card-monthly-summary", "/reports/monthly-summary")]
    [InlineData("reports-index-card-category-totals", "/reports/category-totals")]
    [InlineData("reports-index-card-account-totals", "/reports/account-totals")]
    [InlineData("reports-index-card-account-group-totals", "/reports/account-group-totals")]
    public void Authorized_User_Can_Open_Each_Report_From_Reports_Index(string cardTestId, string expectedRoute)
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();
        var nav = Services.GetRequiredService<FakeNavigationManager>();

        cut.Find($"[data-testid='{cardTestId}']").Click();

        nav.Uri.Should().EndWith(expectedRoute);
    }

    [Fact]
    public void Reports_Index_Uses_Default_Resources_When_A_Culture_Specific_Resource_Is_Unavailable()
    {
        using var _ = UseCulture("fr-FR");

        var cut = RenderAuthorizedIndex();

        cut.Markup.Should().Contain("Financial Snapshot");
        cut.Markup.Should().Contain("Economic State");
    }

    [Fact]
    public void Reports_Index_Does_Not_Duplicate_Asset_Total_Balance_Entry()
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();

        cut.FindAll("[data-testid='reports-index-card-asset-total-balance']").Should().BeEmpty();
    }

    [Theory]
    [InlineData("reports-index-card-monthly-summary")]
    [InlineData("reports-index-card-category-totals")]
    [InlineData("reports-index-card-account-totals")]
    [InlineData("reports-index-card-account-group-totals")]
    public void Reports_Index_Uses_Two_Column_Desktop_Layout_For_Two_Card_Families(string cardTestId)
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();

        cut.Find($"[data-testid='{cardTestId}']").ParentElement!.ClassList
            .Should().Contain("col-lg-6");
    }

    [Fact]
    public void Reports_Index_Does_Not_Show_Monthly_Evolution_Card()
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();

        cut.Markup.Should().NotContain("Monthly Evolution");
    }

    [Fact]
    public void Authorized_User_Sees_Flow_Vs_Stock_Semantic_Disclaimer()
    {
        using var _ = UseCulture("en-US");

        var cut = RenderAuthorizedIndex();

        cut.Markup.Should().Contain("Flow metrics");
        cut.Markup.Should().Contain("stock metrics");
        cut.Markup.Should().Contain("Period Net Result");
        cut.Markup.Should().Contain("Asset Balance");
    }

    [Fact]
    public void Unauthenticated_User_Does_Not_See_Report_Families()
    {
        using var _ = UseCulture("en-US");

        this.AddTestAuthorization();
        ConfigureServices();

        var cut = RenderComponent<ReportsIndexPage>();

        cut.FindAll("[data-testid='reports-index-family']").Should().BeEmpty();
        cut.Markup.Should().Contain("Please log in to view reports.");
    }

    private IRenderedComponent<ReportsIndexPage> RenderAuthorizedIndex()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");
        ConfigureServices();

        return RenderComponent<ReportsIndexPage>();
    }

    private void ConfigureServices()
    {
        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri("http://localhost")
        };

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token)
        {
            _token = token;
        }

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken)
        {
            _token = accessToken;
        }

        public void Clear()
        {
            _token = null;
        }

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct)
        {
            return Task.FromResult(_token);
        }
    }
}
