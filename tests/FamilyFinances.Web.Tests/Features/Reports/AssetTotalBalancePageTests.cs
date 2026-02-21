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

public sealed class AssetTotalBalancePageTests : TestContext
{
    [Fact]
    public void LoadReport_Displays_Total_And_AccountsCount()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/asset-total-balance?asOf=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new AssetTotalBalanceDto(
                    new DateOnly(2026, 1, 31),
                    123_456,
                    3))
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

        var cut = RenderComponent<AssetTotalBalancePage>();
        cut.Find("button.btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Asset Accounts Included");
            cut.Markup.Should().Contain("As of 2026-01-31");
            cut.Markup.Should().Contain("1.234,56\u20AC");
        });
    }

    [Fact]
    public void Unauthenticated_User_Sees_Login_Message()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore(null);
        var authContext = this.AddTestAuthorization();
        authContext.SetNotAuthorized();

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        var cut = RenderComponent<AssetTotalBalancePage>();

        cut.Markup.Should().Contain("Please log in to view reports.");
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
