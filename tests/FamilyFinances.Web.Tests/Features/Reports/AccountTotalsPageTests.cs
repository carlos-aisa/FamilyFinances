using System.Net;
using System.Net.Http.Json;
using System.Globalization;
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

public sealed class AccountTotalsPageTests : TestContext
{
    [Fact]
    public void Default_View_Shows_Period_Totals_Tab_And_Filters()
    {
        RegisterAuthorizedServices(CreateHttpClientSuccessStub(), Array.Empty<AccountDto>());

        var cut = RenderComponent<AccountTotalsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Period Totals");
            cut.Markup.Should().Contain("State Evolution");
            cut.Markup.Should().Contain("Quick Select");
            cut.Markup.Should().Contain("Load Report");
        });
    }

    [Fact]
    public void State_Evolution_Tab_Loads_Accounts_Evolution_Content()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var requestedUris = new List<string>();

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
                requestedUris.Add(uri);

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccountsPayload(currentYear))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(
            httpClient,
            new[]
            {
                new AccountDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null)
            });

        var cut = RenderComponent<AccountTotalsPage>();
        var stateTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("State Evolution"));
        stateTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=accounts"));
            cut.Markup.Should().Contain("Accounts Overview");
            cut.Find("[data-testid='annual-accounts-evolution-chart']");
        });
    }

    [Fact]
    public void State_Evolution_Composition_Chart_Shows_Total_Percentage_Near_100()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var requestedUris = new List<string>();

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
                requestedUris.Add(uri);

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccountsPayloadForComposition(currentYear))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(
            httpClient,
            new[]
            {
                new AccountDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Food",
                    AccountNature.Expense,
                    AccountKind.Other,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null),
                new AccountDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Transport",
                    AccountNature.Expense,
                    AccountKind.Other,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null),
                new AccountDto(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "Salary",
                    AccountNature.Income,
                    AccountKind.Other,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null)
            });

        var cut = RenderComponent<AccountTotalsPage>();
        var stateTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("State Evolution"));
        stateTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=accounts"));
            cut.Find("[data-testid='annual-accounts-evolution-chart']");
        });

        var compositionModeButton = cut.FindAll("button")
            .First(button => button.TextContent.Trim() == "Composition");
        compositionModeButton.Click();

        cut.WaitForAssertion(() =>
        {
            var chart = cut.Find("[data-testid='annual-accounts-composition-chart']");
            var rawTotal = chart.GetAttribute("data-total-percentage");

            rawTotal.Should().NotBeNullOrWhiteSpace();
            decimal.Parse(rawTotal!, CultureInfo.InvariantCulture)
                .Should().BeApproximately(100m, 0.01m);
        });
    }

    private void RegisterAuthorizedServices(HttpClient httpClient, IReadOnlyList<AccountDto> accounts)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton(accountsApiMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
    }

    private static HttpClient CreateHttpClientSuccessStub()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new AccountTotalsDto(
                    DateHelper.GetCurrentMonthStart(),
                    DateHelper.GetCurrentMonthEnd(),
                    Array.Empty<AccountTotalItemDto>()))
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    private static MonthlyEvolutionReportDto CreateAccountsPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.Accounts,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "account:11111111-1111-1111-1111-111111111111",
                    "Main Bank",
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 10_000, 1_000, 1_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 9_500, -500, 500)
                    })
            });
    }

    private static MonthlyEvolutionReportDto CreateAccountsPayloadForComposition(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.Accounts,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "account:11111111-1111-1111-1111-111111111111",
                    "Food",
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 300, 300, 300),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 600, 300, 600)
                    }),
                new MonthlyEvolutionSeriesDto(
                    "account:22222222-2222-2222-2222-222222222222",
                    "Transport",
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 200, 200, 200),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 400, 200, 400)
                    }),
                new MonthlyEvolutionSeriesDto(
                    "account:33333333-3333-3333-3333-333333333333",
                    "Salary",
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), -1000, -1000, -1000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), -2000, -1000, -2000)
                    })
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
