using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class ReportResponsiveLayoutTests : WebTestContext
{
    [Fact]
    public void EconomicState_Snapshot_Uses_Responsive_Grid_And_Global_Filters()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                if (uri.Contains("api/v1/reports/economic-state?asOf=", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new EconomicStateDto(
                            new DateOnly(2026, 2, 21),
                            2_400_000,
                            900_000,
                            1_500_000,
                            120_000,
                            -80_000,
                            40_000))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyBalanceChartDto(
                            2026,
                            2,
                            [new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 100_000)]))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<EconomicStatePage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("col-12 col-md-6 col-xl-4");
            cut.Markup.Should().Contain("ff-report-filter-panel");
            cut.Markup.Should().Contain("ff-premium-tabs");
            cut.Find("[data-testid='economic-state-global-filters']");
            cut.Find("[data-testid='economic-state-global-year']");
            cut.Find("[data-testid='economic-state-global-focused-month']");
            cut.Find("[data-testid='economic-state-global-load']");
        });
    }

    [Fact]
    public void AccountStateEvolution_Uses_Responsive_Overview_Layout()
    {
        var currentYear = DateHelper.GetCurrentYear();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                if (uri.Contains("api/v1/reports/state-evolution", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains($"year={currentYear}", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains("scope=accounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyEvolutionReportDto(
                            currentYear,
                            MonthlyEvolutionScope.Accounts,
                            [
                                new MonthlyEvolutionSeriesDto(
                                    "account:11111111-1111-1111-1111-111111111111",
                                    "Main Bank",
                                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                                    "account",
                                    [
                                        new MonthlyEvolutionPointDto(1, new DateOnly(currentYear, 1, 31), 100_000, 100_000, 100_000)
                                    ])
                            ]))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(
            httpClient,
            [
                new AccountDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null)
            ]);

        var cut = RenderComponent<AccountTotalsPage>();
        cut.FindAll("button.nav-link").First(button => button.TextContent.Contains("state", StringComparison.OrdinalIgnoreCase)).Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("accounts-overview-grid");
            cut.Markup.Should().Contain("col-12 col-xxl-8");
            cut.Markup.Should().Contain("col-12 col-xxl-4");
            cut.Markup.Should().Contain("aria-label=\"Year\"");
            cut.Markup.Should().Contain("ff-data-table");
            cut.Markup.Should().Contain("ff-chart-panel");
        });
    }

    private void RegisterAuthorizedServices(HttpClient httpClient, IReadOnlyList<AccountDto>? accounts = null)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        if (accounts is not null)
        {
            var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
            accountsApiMock
                .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accounts);
            Services.AddSingleton(accountsApiMock.Object);
        }
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
