using System.Net;
using System.Net.Http.Json;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class MonthlySummaryPageTests : WebTestContext
{
    private static IElement GetLoadReportButton(IRenderedComponent<MonthlySummaryPage> cut)
    {
        return cut.FindAll("button")
            .First(button =>
                button.TextContent.Contains("load", StringComparison.OrdinalIgnoreCase) &&
                button.TextContent.Contains("report", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LoadReport_WithSelectedAccount_LoadsMonthlySummary_And_AccountMonthlyChart()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
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

                if (uri.Contains("api/v1/accounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccounts(accountId))
                    };
                }

                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(Array.Empty<PayeeDto>())
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            From: new DateOnly(2026, 2, 1),
                            To: new DateOnly(2026, 3, 1),
                            IncomeTotal: 125_000,
                            ExpenseTotal: -35_000,
                            Net: 90_000,
                            TransactionsCount: 8))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyBalanceChartDto(
                            2026,
                            2,
                            [
                                new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 500_000),
                                new MonthlyChartPointDto(2, new DateOnly(2026, 2, 2), 510_000)
                            ]))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/pareto", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateParetoInsights(dimension))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/anomalies", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAnomalyInsights(dimension))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlySummaryPage>();

        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[0].Change("2026-02-01"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[1].Change("2026-03-01"));
        await cut.InvokeAsync(() => cut.FindAll("select.form-select")[0].Change(accountId.ToString()));
        await cut.InvokeAsync(() => GetLoadReportButton(cut).Click());

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase));

            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/monthly-charts/balance?year=2026&month=2", StringComparison.OrdinalIgnoreCase) &&
                uri.Contains($"accountId={accountId}", StringComparison.OrdinalIgnoreCase));

            cut.Find("[data-testid='monthly-summary-account-monthly-chart']");
        });
    }

    [Fact]
    public async Task LoadReport_WithSelectedAccount_AndPayee_PassesPayeeToAccountMonthlyChart()
    {
        var accountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var payeeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
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

                if (uri.Contains("api/v1/accounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccounts(accountId))
                    };
                }

                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new[] { new PayeeDto(payeeId, "Mercadona") })
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            From: new DateOnly(2026, 2, 1),
                            To: new DateOnly(2026, 3, 1),
                            IncomeTotal: 125_000,
                            ExpenseTotal: -35_000,
                            Net: 90_000,
                            TransactionsCount: 8))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyBalanceChartDto(
                            2026,
                            2,
                            [new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 500_000)]))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/pareto", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateParetoInsights(dimension))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/anomalies", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAnomalyInsights(dimension))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlySummaryPage>();

        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[0].Change("2026-02-01"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[1].Change("2026-03-01"));
        await cut.InvokeAsync(() => cut.FindAll("select.form-select")[0].Change(accountId.ToString()));
        await cut.InvokeAsync(() => cut.FindAll("select.form-select")[1].Change(payeeId.ToString()));
        await cut.InvokeAsync(() => GetLoadReportButton(cut).Click());

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/monthly-charts/balance?year=2026&month=2", StringComparison.OrdinalIgnoreCase) &&
                uri.Contains($"accountId={accountId}", StringComparison.OrdinalIgnoreCase) &&
                uri.Contains($"payeeId={payeeId}", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task LoadReport_WithoutSelectedAccount_DoesNotLoadAccountMonthlyChart()
    {
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

                if (uri.Contains("api/v1/accounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccounts(Guid.NewGuid()))
                    };
                }

                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(Array.Empty<PayeeDto>())
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            From: new DateOnly(2026, 2, 1),
                            To: new DateOnly(2026, 3, 1),
                            IncomeTotal: 125_000,
                            ExpenseTotal: -35_000,
                            Net: 90_000,
                            TransactionsCount: 8))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/pareto", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateParetoInsights(dimension))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/anomalies", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAnomalyInsights(dimension))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlySummaryPage>();

        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[0].Change("2026-02-01"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[1].Change("2026-03-01"));
        await cut.InvokeAsync(() => GetLoadReportButton(cut).Click());

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase));

            requestedUris.Should().NotContain(uri =>
                uri.Contains("api/v1/reports/monthly-charts/balance", StringComparison.OrdinalIgnoreCase));

            cut.FindAll("[data-testid='monthly-summary-account-monthly-chart']").Should().BeEmpty();
        });
    }

    [Fact]
    public async Task InsightPanel_TogglesDimension_AndRequestsPayeeInsights()
    {
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

                if (uri.Contains("api/v1/accounts", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAccounts(Guid.NewGuid()))
                    };
                }

                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(Array.Empty<PayeeDto>())
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            From: new DateOnly(2026, 2, 1),
                            To: new DateOnly(2026, 3, 1),
                            IncomeTotal: 125_000,
                            ExpenseTotal: -35_000,
                            Net: 90_000,
                            TransactionsCount: 8))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/pareto", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateParetoInsights(dimension))
                    };
                }

                if (uri.Contains("api/v1/reports/insights/anomalies", StringComparison.OrdinalIgnoreCase))
                {
                    var dimension = uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase)
                        ? ReportingInsightDimension.Payee
                        : ReportingInsightDimension.Group;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(CreateAnomalyInsights(dimension))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        RegisterAuthorizedServices(httpClient);

        var cut = RenderComponent<MonthlySummaryPage>();

        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[0].Change("2026-02-01"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[1].Change("2026-03-01"));
        await cut.InvokeAsync(() => GetLoadReportButton(cut).Click());

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='monthly-summary-insights-panel']");
            cut.Markup.Should().Contain("Food");
        });

        await cut.InvokeAsync(() =>
        {
            var insightButtons = cut.Find("[data-testid='monthly-summary-insights-panel']")
                .QuerySelectorAll(".btn-group .btn");
            insightButtons[1].Click();
        });

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/insights/pareto", StringComparison.OrdinalIgnoreCase) &&
                uri.Contains("dimension=payee", StringComparison.OrdinalIgnoreCase));
        });
    }

    private void RegisterAuthorizedServices(HttpClient httpClient)
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
        Services.AddScoped<IAccountsApi, AccountsApi>();
        Services.AddScoped<PayeesApi>();
    }

    private static IReadOnlyList<AccountDto> CreateAccounts(Guid selectedAccountId)
    {
        return
        [
            new AccountDto(
                selectedAccountId,
                "Main Income",
                AccountNature.Income,
                AccountKind.Other,
                new DateOnly(2020, 1, 1),
                false,
                null)
        ];
    }

    private static ReportingParetoInsightsDto CreateParetoInsights(ReportingInsightDimension dimension)
    {
        return new ReportingParetoInsightsDto(
            From: new DateOnly(2026, 2, 1),
            To: new DateOnly(2026, 3, 1),
            Dimension: dimension,
            Expense: new ParetoInsightSectionDto(
                AccountNature.Expense,
                35_000,
                5,
                35_000,
                100m,
                [new ParetoContributorDto(Guid.NewGuid(), dimension == ReportingInsightDimension.Payee ? "Mercadona" : "Food", 35_000, 100m)]),
            Income: new ParetoInsightSectionDto(
                AccountNature.Income,
                125_000,
                5,
                125_000,
                100m,
                [new ParetoContributorDto(Guid.NewGuid(), dimension == ReportingInsightDimension.Payee ? "Employer" : "Salary", 125_000, 100m)]));
    }

    private static ReportingAnomalyInsightsDto CreateAnomalyInsights(ReportingInsightDimension dimension)
    {
        return new ReportingAnomalyInsightsDto(
            Year: 2026,
            Month: 2,
            Nature: AccountNature.Expense,
            Dimension: dimension,
            RequiredHistoryMonths: 3,
            ThresholdRule: "Anomaly if current amount is above baseline + 2σ (or baseline x1.25 when σ=0).",
            Contributors:
            [
                new AnomalyContributorDto(
                    Guid.NewGuid(),
                    dimension == ReportingInsightDimension.Payee ? "Mercadona" : "Food",
                    35_000,
                    20_000,
                    30_000,
                    2.5m,
                    true,
                    false,
                    6,
                    "threshold exceeded")
            ]);
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
