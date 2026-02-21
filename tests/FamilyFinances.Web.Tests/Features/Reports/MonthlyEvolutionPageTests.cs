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

public sealed class MonthlyEvolutionPageTests : TestContext
{
    [Fact]
    public void Initial_Load_Uses_AssetTotal_And_Renders_Table()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/monthly-evolution") &&
                    req.RequestUri!.ToString().Contains($"year={currentYear}") &&
                    req.RequestUri!.ToString().Contains("scope=asset-total")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(CreateAssetTotalPayload(currentYear))
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Monthly Evolution");
            cut.Markup.Should().Contain("Asset Total");
            cut.Markup.Should().Contain("Latest End Balance");
        });
    }

    [Fact]
    public void Switching_Scope_Triggers_Reload_And_Renders_Accounts_Data()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var requestedUris = new List<string>();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                requestedUris.Add(req.RequestUri!.ToString());

                var payload = req.RequestUri!.ToString().Contains("scope=accounts")
                    ? CreateAccountsPayload(currentYear)
                    : CreateAssetTotalPayload(currentYear);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                };
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();
        cut.WaitForAssertion(() => requestedUris.Should().Contain(uri => uri.Contains("scope=asset-total")));

        var accountsTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("Accounts"));
        accountsTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=accounts"));
            cut.Markup.Should().Contain("Main Bank");
        });
    }

    [Fact]
    public void Changing_Year_Triggers_Reload_With_Selected_Year()
    {
        var currentYear = DateHelper.GetCurrentYear();
        var targetYear = currentYear - 1;
        var requestedUris = new List<string>();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                requestedUris.Add(req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(CreateAssetTotalPayload(currentYear))
                };
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();
        cut.WaitForAssertion(() => requestedUris.Should().Contain(uri => uri.Contains($"year={currentYear}")));

        cut.Find("select.form-select").Change(targetYear.ToString());

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains($"year={targetYear}"));
        });
    }

    [Fact]
    public void Shows_Loading_State_While_Request_Is_In_Flight()
    {
        var responseTcs = new TaskCompletionSource<HttpResponseMessage>();
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => responseTcs.Task);

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();
        cut.Markup.Should().Contain("Loading report data...");

        responseTcs.SetResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(CreateAssetTotalPayload(DateHelper.GetCurrentYear()))
        });

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Latest End Balance");
        });
    }

    [Fact]
    public void Shows_Error_State_When_Request_Fails()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Failed to load report");
        });
    }

    [Fact]
    public void Accounts_View_Shows_One_Row_Per_Series_And_Expands_Month_Details_On_Demand()
    {
        var currentYear = DateHelper.GetCurrentYear();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var payload = req.RequestUri!.ToString().Contains("scope=accounts")
                    ? CreateAccountsPayload(currentYear)
                    : CreateAssetTotalPayload(currentYear);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                };
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlyEvolutionPage>();

        var accountsTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("Accounts"));
        accountsTab.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Find("select.month-focus-select");
            cut.Find("button.btn.btn-sm.btn-outline-secondary").TextContent.Should().Contain("View months");
        });

        cut.WaitForAssertion(() =>
        {
            cut.FindAll("tr.series-summary-row").Count.Should().Be(1);
            cut.FindAll("tr.series-detail-row").Count.Should().Be(0);
        });

        cut.Find("button.btn.btn-sm.btn-outline-secondary").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button.btn.btn-sm.btn-outline-secondary").TextContent.Should().Contain("Hide months");
            cut.FindAll("tr.series-summary-row").Count.Should().Be(1);
            cut.FindAll("tr.series-detail-row").Count.Should().Be(1);
            cut.Markup.Should().Contain("January");
            cut.Markup.Should().Contain("February");
        });
    }

    [Fact]
    public void Accounts_View_Groups_Series_By_Account_Nature()
    {
        var currentYear = DateHelper.GetCurrentYear();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var payload = req.RequestUri!.ToString().Contains("scope=accounts")
                    ? CreateAccountsPayloadWithMixedNatures(currentYear)
                    : CreateAssetTotalPayload(currentYear);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                };
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
                    null),
                new AccountDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Salary",
                    AccountNature.Income,
                    AccountKind.IncomeSource,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null)
            });

        var cut = RenderComponent<MonthlyEvolutionPage>();

        var accountsTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("Accounts"));
        accountsTab.Click();

        cut.WaitForAssertion(() =>
        {
            var groupTitles = cut.FindAll(".account-nature-title")
                .Select(e => e.TextContent.Trim())
                .ToList();

            groupTitles.Should().Contain(title => title.Contains("Asset"));
            groupTitles.Should().Contain(title => title.Contains("Income"));
            groupTitles.Should().Contain(title => title.Contains("Uncategorized"));
        });
    }

    [Fact]
    public void Accounts_View_Summary_Uses_Asset_Series_When_All_Natures_Net_To_Zero()
    {
        var currentYear = DateHelper.GetCurrentYear();

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = CreateHttpClient(handlerMock);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var payload = req.RequestUri!.ToString().Contains("scope=accounts")
                    ? CreateBalancedAccountsPayload(currentYear)
                    : CreateAssetTotalPayload(currentYear);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                };
            });

        RegisterAuthorizedServices(
            httpClient,
            new[]
            {
                new AccountDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Cash",
                    AccountNature.Asset,
                    AccountKind.Cash,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null),
                new AccountDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Opening Equity",
                    AccountNature.Equity,
                    AccountKind.Other,
                    new DateOnly(currentYear, 1, 1),
                    false,
                    null)
            });

        var cut = RenderComponent<MonthlyEvolutionPage>();

        var accountsTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("Accounts"));
        accountsTab.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Latest Asset End Balance");
            var summaryCards = cut.FindAll("div.row.g-3.mb-4 div.card-body");
            summaryCards[0].TextContent.Should().Contain("+€26.00");
        });
    }

    private void RegisterAuthorizedServices(
        HttpClient httpClient,
        IReadOnlyList<AccountDto>? accounts = null)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts ?? Array.Empty<AccountDto>());

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton(accountsApiMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
    }

    private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
    {
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    private static MonthlyEvolutionReportDto CreateAssetTotalPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
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
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 12_000, 2_000, 2_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 12_500, 500, 2_500)
                    })
            });
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

    private static MonthlyEvolutionReportDto CreateAccountsPayloadWithMixedNatures(int year)
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
                    }),
                new MonthlyEvolutionSeriesDto(
                    "account:22222222-2222-2222-2222-222222222222",
                    "Salary",
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 2_000, 2_000, 2_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 5_000, 3_000, 5_000)
                    }),
                new MonthlyEvolutionSeriesDto(
                    "account:33333333-3333-3333-3333-333333333333",
                    "Unknown Account",
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 1_000, 1_000, 1_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 800, -200, 800)
                    })
            });
    }

    private static MonthlyEvolutionReportDto CreateBalancedAccountsPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.Accounts,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "account:11111111-1111-1111-1111-111111111111",
                    "Cash",
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 2_600, 2_600, 2_600)
                    }),
                new MonthlyEvolutionSeriesDto(
                    "account:22222222-2222-2222-2222-222222222222",
                    "Opening Equity",
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "account",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), -2_600, -2_600, -2_600)
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
