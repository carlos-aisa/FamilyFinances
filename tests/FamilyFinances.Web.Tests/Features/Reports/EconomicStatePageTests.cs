using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
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
