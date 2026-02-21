using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class ReportsIndexPageTests : TestContext
{
    [Fact]
    public void Authorized_User_Can_Open_Account_Group_Totals_From_Reports_Index()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<ReportsIndexPage>();

        var accountGroupCard = cut
            .FindAll(".report-card")
            .First(card => card.TextContent.Contains("Account Group Totals"));

        accountGroupCard.TextContent.Should().Contain("Account Group Totals");

        accountGroupCard.Click();

        nav.Uri.Should().EndWith("/reports/account-group-totals");
    }

    [Fact]
    public void Reports_Index_Does_Not_Show_Monthly_Evolution_Card()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var cut = RenderComponent<ReportsIndexPage>();

        cut.Markup.Should().NotContain("Monthly Evolution");
    }

    [Fact]
    public void Authorized_User_Can_Open_Economic_State_From_Reports_Index()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<ReportsIndexPage>();

        var economicStateCard = cut
            .FindAll(".report-card")
            .First(card => card.TextContent.Contains("Economic State"));

        economicStateCard.Click();

        nav.Uri.Should().EndWith("/reports/economic-state");
    }

    [Fact]
    public void Authorized_User_Sees_Flow_Vs_Stock_Semantic_Disclaimer()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var cut = RenderComponent<ReportsIndexPage>();

        cut.Markup.Should().Contain("Flow metrics");
        cut.Markup.Should().Contain("stock metrics");
        cut.Markup.Should().Contain("Period Net Result");
        cut.Markup.Should().Contain("Asset Balance");
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

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
