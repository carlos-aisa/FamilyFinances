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
    public void Authorized_User_Can_Open_Asset_Total_Balance_From_Reports_Index()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<ReportsIndexPage>();

        var assetCard = cut
            .FindAll(".report-card")
            .First(card => card.TextContent.Contains("Asset Total Balance"));

        assetCard.TextContent.Should().Contain("Asset Total Balance");

        assetCard.Click();

        nav.Uri.Should().EndWith("/reports/asset-total-balance");
    }

    [Fact]
    public void Authorized_User_Can_Open_Monthly_Evolution_From_Reports_Index()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IHttpClientFactory>(new TestHttpClientFactory());
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<ReportsIndexPage>();

        var monthlyEvolutionCard = cut
            .FindAll(".report-card")
            .First(card => card.TextContent.Contains("Monthly Evolution"));

        monthlyEvolutionCard.TextContent.Should().Contain("Monthly Evolution");

        monthlyEvolutionCard.Click();

        nav.Uri.Should().EndWith("/reports/monthly-evolution");
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
