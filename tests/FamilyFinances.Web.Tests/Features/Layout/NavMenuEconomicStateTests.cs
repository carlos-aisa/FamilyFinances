using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Layout;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Layout;

public sealed class NavMenuEconomicStateTests : WebTestContext
{
    public NavMenuEconomicStateTests()
    {
    }

    [Fact]
    public void Authorized_User_Sees_EconomicState_Link_And_Asset_Preview()
    {
        using var _ = UseCulture("en-US");

        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var payload = new EconomicStateDto(
            AsOf: new DateOnly(2026, 2, 21),
            AssetsTotalCents: 1_234_56,
            LiabilitiesTotalCents: 500_00,
            NetWorthCents: 734_56,
            IncomeTotalCents: 100_00,
            ExpenseTotalCents: -20_00,
            PeriodNetResultCents: 80_00);

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/economic-state?asOf=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<NavMenu>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Economic State");
            cut.Markup.Should().Contain("Quick Entry");
            cut.Markup.Should().Contain("href=\"quick-entry\"");
            cut.Markup.Should().Contain("Asset Balance");
            cut.Markup.Should().Contain("As of");
            cut.Markup.Should().Contain(payload.AsOf.ToString("d", CultureInfo.CurrentCulture));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCents(payload.AssetsTotalCents));
            cut.Markup.Should().Contain("ff-nav-link");
            cut.Markup.Should().Contain("data-testid=\"nav-settings-link\"");
            cut.Markup.Should().NotContain("settings-language-selector");
        });
    }

    [Fact]
    public void Unauthenticated_User_Does_Not_See_EconomicState_Section()
    {
        using var _ = UseCulture("en-US");

        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(new HttpClient { BaseAddress = new Uri("http://localhost:5000") });

        var tokenStore = new TestTokenStore(null);
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<NavMenu>();

        cut.Markup.Should().Contain("Login");
        cut.Markup.Should().NotContain("Economic State");
        cut.Markup.Should().NotContain("Asset Balance");
    }

    [Fact]
    public void EconomicState_Link_Navigates_To_EconomicState_Report()
    {
        using var _ = UseCulture("en-US");

        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var payload = new EconomicStateDto(
            AsOf: new DateOnly(2026, 2, 21),
            AssetsTotalCents: 1_234_56,
            LiabilitiesTotalCents: 500_00,
            NetWorthCents: 734_56,
            IncomeTotalCents: 100_00,
            ExpenseTotalCents: -20_00,
            PeriodNetResultCents: 80_00);

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/economic-state?asOf=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<NavMenu>();

        cut.WaitForAssertion(() =>
        {
            var link = cut.Find("a[href='reports/economic-state']");
            var href = link.GetAttribute("href");
            href.Should().Be("reports/economic-state");

            nav.NavigateTo(href!);
            nav.Uri.Should().EndWith("/reports/economic-state");
        });
    }

    [Fact]
    public void Asset_Preview_Refreshes_After_Token_Becomes_Available_And_Location_Changes()
    {
        using var _ = UseCulture("en-US");

        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var payload = new EconomicStateDto(
            AsOf: new DateOnly(2026, 2, 21),
            AssetsTotalCents: 1_234_56,
            LiabilitiesTotalCents: 500_00,
            NetWorthCents: 734_56,
            IncomeTotalCents: 100_00,
            ExpenseTotalCents: -20_00,
            PeriodNetResultCents: 80_00);

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/economic-state?asOf=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore(null);
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<NavMenu>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Economic State");
            cut.Markup.Should().Contain("Unavailable");
        });

        tokenStore.SetAccessToken("test-token");
        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo("/reports");

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain(MoneyFormatter.FormatCents(payload.AssetsTotalCents));
            cut.Markup.Should().Contain(payload.AsOf.ToString("d", CultureInfo.CurrentCulture));
        });
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
