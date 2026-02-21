using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class EconomicStatePageTests : TestContext
{
    [Fact]
    public void EconomicStatePage_Loads_With_Current_Date_By_Default()
    {
        var expectedAsOf = DateOnly.FromDateTime(DateTime.Today);
        HttpRequestMessage? capturedRequest = null;

        var (httpClientFactory, _) = BuildHttpClientFactoryForEconomicState((req, _) => capturedRequest = req);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<EconomicStatePage>();

        cut.WaitForAssertion(() =>
        {
            capturedRequest.Should().NotBeNull();
            capturedRequest!.RequestUri!.ToString()
                .Should().Contain($"api/v1/reports/economic-state?asOf={expectedAsOf:yyyy-MM-dd}");
        });
    }

    [Fact]
    public void EconomicStatePage_Displays_Stock_And_Flow_Kpis()
    {
        var (httpClientFactory, _) = BuildHttpClientFactoryForEconomicState();

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<EconomicStatePage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Asset Balance");
            cut.Markup.Should().Contain("Liability Balance");
            cut.Markup.Should().Contain("Net Worth");
            cut.Markup.Should().Contain("Income");
            cut.Markup.Should().Contain("Expense");
            cut.Markup.Should().Contain("Period Net Result");
            cut.Markup.Should().Contain("Stock metrics");
            cut.Markup.Should().Contain("Flow metrics");
        });
    }

    [Fact]
    public void Asset_Evolution_Tab_Loads_Asset_Total_Evolution_Data()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var requestedUris = new List<string>();

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var economicPayload = new EconomicStateDto(
            AsOf: new DateOnly(currentYear, 2, 21),
            AssetsTotalCents: 2_457_005,
            LiabilitiesTotalCents: 1_000_000,
            NetWorthCents: 1_457_005,
            IncomeTotalCents: 134_346,
            ExpenseTotalCents: -189_895,
            PeriodNetResultCents: -55_549);

        var evolutionPayload = new MonthlyEvolutionReportDto(
            currentYear,
            MonthlyEvolutionScope.AssetTotal,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "asset-total",
                    "Asset Total",
                    null,
                    "scope",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(currentYear, 1, 31), 2_532_894, 2_532_894, 2_532_894),
                        new MonthlyEvolutionPointDto(2, new DateOnly(currentYear, 2, DateTime.DaysInMonth(currentYear, 2)), 2_457_005, -75_889, 2_457_005)
                    })
            });

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                requestedUris.Add(uri);

                if (uri.Contains("api/v1/reports/economic-state?asOf="))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(economicPayload)
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=asset-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(evolutionPayload)
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
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

        var cut = RenderComponent<EconomicStatePage>();
        var assetTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("Asset Evolution"));
        assetTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=asset-total"));
            cut.Markup.Should().Contain("Asset Total (Monthly Overview)");
            cut.Find("[data-testid='economic-state-asset-evolution-chart']");
        });
    }

    [Fact]
    public void Unauthenticated_User_Sees_Login_Message()
    {
        var (httpClientFactory, _) = BuildHttpClientFactoryForEconomicState();

        var tokenStore = new TestTokenStore(null);
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<EconomicStatePage>();

        cut.Markup.Should().Contain("Please log in to view reports.");
    }

    private static (Mock<IHttpClientFactory> Factory, Mock<HttpMessageHandler> Handler) BuildHttpClientFactoryForEconomicState(
        Action<HttpRequestMessage, CancellationToken>? onRequest = null)
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var payload = new EconomicStateDto(
            AsOf: new DateOnly(2026, 1, 31),
            AssetsTotalCents: 320_000,
            LiabilitiesTotalCents: 150_000,
            NetWorthCents: 170_000,
            IncomeTotalCents: 100_000,
            ExpenseTotalCents: -30_000,
            PeriodNetResultCents: 70_000);

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/economic-state?asOf=")),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => onRequest?.Invoke(req, ct))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        return (httpClientFactory, httpMessageHandlerMock);
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
