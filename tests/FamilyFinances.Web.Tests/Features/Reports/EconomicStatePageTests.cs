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
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class EconomicStatePageTests : WebTestContext
{
    private static readonly Guid AssetAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid IncomeAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ExpenseAccountId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void EconomicStatePage_Loads_With_Current_Date_By_Default()
    {
        var expectedAsOf = DateOnly.FromDateTime(DateTime.Today);
        var requestedUris = new List<string>();

        var (httpClientFactory, _) = BuildHttpClientFactoryForEconomicState((req, _) => requestedUris.Add(req.RequestUri!.ToString()));

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
        Services.AddSingleton(BuildAccountsApiMock().Object);
        var cut = RenderComponent<EconomicStatePage>();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri =>
                uri.Contains($"api/v1/reports/economic-state?asOf={expectedAsOf:yyyy-MM-dd}"));
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
        Services.AddSingleton(BuildAccountsApiMock().Object);
        var cut = RenderComponent<EconomicStatePage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Asset balance");
            cut.Markup.Should().Contain("Liability balance");
            cut.Markup.Should().Contain("Net worth");
            cut.Markup.Should().Contain("Income");
            cut.Markup.Should().Contain("Expense");
            cut.Markup.Should().Contain("Period net result");
            cut.Markup.Should().Contain("Month-focused income vs expense");
            cut.Markup.Should().Contain("Monthly net list (Income - Expense)");
            cut.Find("[data-testid='economic-state-annual-income-expense-bars']");
            cut.Markup.Should().Contain("ff-premium-tabs");
            cut.Markup.Should().Contain("ff-kpi-card");
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

        MonthlyBalanceChartDto BuildMonthlyChartPayload(int month)
        {
            var daysInMonth = DateTime.DaysInMonth(currentYear, month);
            return new MonthlyBalanceChartDto(
                currentYear,
                month,
                Enumerable.Range(1, daysInMonth)
                    .Select(day => new MonthlyChartPointDto(
                        day,
                        new DateOnly(currentYear, month, day),
                        2_400_000 + (day * 1_000)))
                    .ToArray());
        }

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

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=asset-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(evolutionPayload)
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildAccountsEvolutionPayload(currentYear))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance") &&
                    uri.Contains($"year={currentYear}"))
                {
                    var month = 1;
                    var monthParam = uri.Split('&')
                        .FirstOrDefault(part => part.Contains("month=", StringComparison.OrdinalIgnoreCase));

                    if (monthParam is not null &&
                        int.TryParse(monthParam.Split('=').LastOrDefault(), out var parsedMonth) &&
                        parsedMonth is >= 1 and <= 12)
                    {
                        month = parsedMonth;
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildMonthlyChartPayload(month))
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
        Services.AddSingleton(BuildAccountsApiMock().Object);

        var exportCall = JSInterop.SetupVoid("familyFinancesCharts.downloadCsv", _ => true);
        var cut = RenderComponent<EconomicStatePage>();
        var assetTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("asset", StringComparison.OrdinalIgnoreCase));
        assetTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=asset-total"));
            requestedUris.Should().Contain(uri => uri.Contains("monthly-charts/balance"));
            cut.Markup.Should().Contain("Monthly Overview");
            cut.Find("[data-testid='economic-state-asset-evolution-chart']");
            cut.Find("[data-testid='economic-state-asset-monthly-chart']");
            var points = cut
                .Find("[data-testid='economic-state-asset-monthly-chart'] [data-series-key='asset-total-monthly']")
                .GetAttribute("data-points");
            points.Should().NotBeNullOrWhiteSpace();
            points!.Should().Contain($"{DateTime.Today.Day}:");
            var lastDayOfCurrentMonth = DateTime.DaysInMonth(currentYear, DateHelper.GetCurrentMonth());
            if (DateTime.Today.Day < lastDayOfCurrentMonth)
            {
                points.Should().NotContain($"{lastDayOfCurrentMonth}:");
            }
            cut.Find("[data-testid='economic-state-global-year']");
            cut.Find("[data-testid='economic-state-global-focused-month']");
            cut.FindAll("[data-testid='economic-state-asset-focused-month']").Should().BeEmpty();
        });

        var monthSelector = cut.Find("[data-testid='economic-state-global-focused-month']");
        monthSelector.Change("1");

        cut.WaitForAssertion(() =>
        {
            requestedUris.Count(uri => uri.Contains("monthly-charts/balance"))
                .Should().BeGreaterThanOrEqualTo(2);
            requestedUris.Should().Contain(uri =>
                uri.Contains("monthly-charts/balance") &&
                uri.Contains("month=1", StringComparison.OrdinalIgnoreCase));
            requestedUris.Should().Contain(uri =>
                uri.Contains("api/v1/reports/economic-state?asOf=", StringComparison.OrdinalIgnoreCase) &&
                uri.Contains($"asOf={currentYear}-01-{DateTime.DaysInMonth(currentYear, 1):00}", StringComparison.OrdinalIgnoreCase));
            var overview = cut.FindAll("table.ff-data-table").First();
            overview.TextContent.Should().Contain("January");
            overview.TextContent.Should().NotContain("February");
            cut.Markup.Should().Contain($"Period: January {currentYear}");
            cut.Markup.Should().Contain("Asset movement");
            cut.Find("[data-testid='economic-state-asset-overview-semantics-hint']").TextContent
                .Should().Contain("can differ from Snapshot Income - Expense");
        });

        cut.Find("[data-testid='economic-state-asset-overview-export-csv']").Click();

        exportCall.Invocations.Should().ContainSingle();
        var csvContent = exportCall.Invocations.Single().Arguments[1] as string;
        csvContent.Should().NotBeNull();
        csvContent!.Should()
            .Contain("January")
            .And.NotContain("February");
    }

    [Fact]
    public void Income_Evolution_Tab_Loads_Income_Total_Evolution_Data()
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
            MonthlyEvolutionScope.IncomeTotal,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "income-total",
                    "Income Total",
                    null,
                    "scope",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(currentYear, 1, 31), 125_000, 125_000, 125_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(currentYear, 2, DateTime.DaysInMonth(currentYear, 2)), 134_346, 9_346, 134_346)
                    })
            });

        MonthlyBalanceChartDto BuildMonthlyChartPayload(int month)
        {
            var daysInMonth = DateTime.DaysInMonth(currentYear, month);
            return new MonthlyBalanceChartDto(
                currentYear,
                month,
                Enumerable.Range(1, daysInMonth)
                    .Select(day => new MonthlyChartPointDto(
                        day,
                        new DateOnly(currentYear, month, day),
                        120_000 + (day * 1_000)))
                    .ToArray());
        }

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

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=income-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(evolutionPayload)
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildAccountsEvolutionPayload(currentYear))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance") &&
                    uri.Contains($"year={currentYear}"))
                {
                    var month = 1;
                    var monthParam = uri.Split('&')
                        .FirstOrDefault(part => part.Contains("month=", StringComparison.OrdinalIgnoreCase));

                    if (monthParam is not null &&
                        int.TryParse(monthParam.Split('=').LastOrDefault(), out var parsedMonth) &&
                        parsedMonth is >= 1 and <= 12)
                    {
                        month = parsedMonth;
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildMonthlyChartPayload(month))
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
        Services.AddSingleton(BuildAccountsApiMock().Object);

        var cut = RenderComponent<EconomicStatePage>();
        var incomeTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("income", StringComparison.OrdinalIgnoreCase));
        incomeTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=income-total"));
            requestedUris.Should().Contain(uri => uri.Contains("monthly-charts/balance") && uri.Contains("nature=Income"));
            cut.Markup.Should().Contain("Monthly Overview");
            cut.Find("[data-testid='economic-state-income-evolution-chart']");
            cut.Find("[data-testid='economic-state-income-monthly-chart']");
            var points = cut
                .Find("[data-testid='economic-state-income-monthly-chart'] [data-series-key='income-total-monthly']")
                .GetAttribute("data-points");
            points.Should().NotBeNullOrWhiteSpace();
            points!.Should().Contain($"{DateTime.Today.Day}:");
            var lastDayOfCurrentMonth = DateTime.DaysInMonth(currentYear, DateHelper.GetCurrentMonth());
            if (DateTime.Today.Day < lastDayOfCurrentMonth)
                points.Should().NotContain($"{lastDayOfCurrentMonth}:");
            cut.Find("[data-testid='economic-state-global-focused-month']");
            cut.FindAll("[data-testid='economic-state-income-focused-month']").Should().BeEmpty();
        });

        cut.Find("[data-testid='economic-state-global-focused-month']").Change("1");

        cut.WaitForAssertion(() =>
        {
            var overview = cut.FindAll("table.ff-data-table").First();
            overview.TextContent.Should().Contain("January");
            overview.TextContent.Should().NotContain("February");
            cut.Markup.Should().Contain($"Period: January {currentYear}");
            cut.Markup.Should().NotContain("Current month:");
        });
    }

    [Fact]
    public void Expense_Evolution_Tab_Loads_Expense_Total_Evolution_Data()
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
            MonthlyEvolutionScope.ExpenseTotal,
            new[]
            {
                new MonthlyEvolutionSeriesDto(
                    "expense-total",
                    "Expense Total",
                    null,
                    "scope",
                    new[]
                    {
                        new MonthlyEvolutionPointDto(1, new DateOnly(currentYear, 1, 31), 125_000, 125_000, 125_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(currentYear, 2, DateTime.DaysInMonth(currentYear, 2)), 134_346, 9_346, 134_346)
                    })
            });

        MonthlyBalanceChartDto BuildMonthlyChartPayload(int month)
        {
            var daysInMonth = DateTime.DaysInMonth(currentYear, month);
            return new MonthlyBalanceChartDto(
                currentYear,
                month,
                Enumerable.Range(1, daysInMonth)
                    .Select(day => new MonthlyChartPointDto(
                        day,
                        new DateOnly(currentYear, month, day),
                        120_000 + (day * 1_000)))
                    .ToArray());
        }

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

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=expense-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(evolutionPayload)
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") &&
                    uri.Contains($"year={currentYear}") &&
                    uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildAccountsEvolutionPayload(currentYear))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance") &&
                    uri.Contains($"year={currentYear}"))
                {
                    var month = 1;
                    var monthParam = uri.Split('&')
                        .FirstOrDefault(part => part.Contains("month=", StringComparison.OrdinalIgnoreCase));

                    if (monthParam is not null &&
                        int.TryParse(monthParam.Split('=').LastOrDefault(), out var parsedMonth) &&
                        parsedMonth is >= 1 and <= 12)
                    {
                        month = parsedMonth;
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildMonthlyChartPayload(month))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            new DateOnly(currentYear, 2, 1),
                            new DateOnly(currentYear, 2, 22),
                            IncomeTotal: 1_000,
                            ExpenseTotal: -500,
                            Net: 500,
                            TransactionsCount: 3))
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
        Services.AddSingleton(BuildAccountsApiMock().Object);

        var cut = RenderComponent<EconomicStatePage>();
        var expenseTab = cut.FindAll("button.nav-link")
            .First(button => button.TextContent.Contains("expense", StringComparison.OrdinalIgnoreCase));
        expenseTab.Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains("scope=expense-total"));
            requestedUris.Should().Contain(uri => uri.Contains("monthly-charts/balance") && uri.Contains("nature=Expense"));
            cut.Markup.Should().Contain("Monthly expense overview");
            cut.Find("[data-testid='economic-state-expense-evolution-chart']");
            cut.Find("[data-testid='economic-state-expense-monthly-chart']");
            var points = cut
                .Find("[data-testid='economic-state-expense-monthly-chart'] [data-series-key='expense-total-monthly']")
                .GetAttribute("data-points");
            points.Should().NotBeNullOrWhiteSpace();
            points!.Should().Contain($"{DateTime.Today.Day}:");
            var lastDayOfCurrentMonth = DateTime.DaysInMonth(currentYear, DateHelper.GetCurrentMonth());
            if (DateTime.Today.Day < lastDayOfCurrentMonth)
                points.Should().NotContain($"{lastDayOfCurrentMonth}:");
            cut.Find("[data-testid='economic-state-global-focused-month']");
            cut.FindAll("[data-testid='economic-state-expense-focused-month']").Should().BeEmpty();
        });

        cut.Find("[data-testid='economic-state-global-focused-month']").Change("1");

        cut.WaitForAssertion(() =>
        {
            var overview = cut.FindAll("table.ff-data-table").First();
            overview.TextContent.Should().Contain("January");
            overview.TextContent.Should().NotContain("February");
            cut.Markup.Should().Contain($"Period: January {currentYear}");
            cut.Markup.Should().NotContain("Current month:");
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
        Services.AddSingleton(BuildAccountsApiMock().Object);

        var cut = RenderComponent<EconomicStatePage>();

        cut.Markup.Should().Contain("Please sign in to access reports.");
    }

    [Fact]
    public void Snapshot_Chart_Payloads_Use_Shared_Income_Expense_Semantic_Colors()
    {
        var (httpClientFactory, _) = BuildHttpClientFactoryForEconomicState();
        var lineCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);
        var barCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualBarChart", _ => true);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
        Services.AddSingleton(BuildAccountsApiMock().Object);

        RenderComponent<EconomicStatePage>();

        lineCall.Invocations.Should().NotBeEmpty();
        barCall.Invocations.Should().NotBeEmpty();

        var payloadColors = new List<string>();
        foreach (var invocation in lineCall.Invocations.Concat(barCall.Invocations))
        {
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(invocation.Arguments[1]);
            using var payload = System.Text.Json.JsonDocument.Parse(payloadJson);
            var datasets = payload.RootElement.GetProperty("datasets");
            payloadColors.AddRange(datasets.EnumerateArray()
                .Select(dataset => dataset.GetProperty("colorHex").GetString())
                .Where(color => !string.IsNullOrWhiteSpace(color))!
                .Select(color => color!));
        }

        payloadColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Income));
        payloadColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Expense));
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
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => onRequest?.Invoke(req, ct))
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();

                if (uri.Contains("api/v1/reports/economic-state?asOf="))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(payload)
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-charts/balance"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyBalanceChartDto(
                            payload.AsOf.Year,
                            payload.AsOf.Month,
                            [new MonthlyChartPointDto(1, new DateOnly(payload.AsOf.Year, payload.AsOf.Month, 1), payload.AssetsTotalCents)]))
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") && uri.Contains("scope=income-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyEvolutionReportDto(
                            payload.AsOf.Year,
                            MonthlyEvolutionScope.IncomeTotal,
                            [
                                new MonthlyEvolutionSeriesDto(
                                    "income-total",
                                    "Income Total",
                                    null,
                                    "scope",
                                    [
                                        new MonthlyEvolutionPointDto(payload.AsOf.Month, new DateOnly(payload.AsOf.Year, payload.AsOf.Month, payload.AsOf.Day), payload.IncomeTotalCents, payload.IncomeTotalCents, payload.IncomeTotalCents)
                                    ])
                            ]))
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") && uri.Contains("scope=expense-total"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlyEvolutionReportDto(
                            payload.AsOf.Year,
                            MonthlyEvolutionScope.ExpenseTotal,
                            [
                                new MonthlyEvolutionSeriesDto(
                                    "expense-total",
                                    "Expense Total",
                                    null,
                                    "scope",
                                    [
                                        new MonthlyEvolutionPointDto(payload.AsOf.Month, new DateOnly(payload.AsOf.Year, payload.AsOf.Month, payload.AsOf.Day), Math.Abs(payload.ExpenseTotalCents), Math.Abs(payload.ExpenseTotalCents), Math.Abs(payload.ExpenseTotalCents))
                                    ])
                            ]))
                    };
                }

                if (uri.Contains("api/v1/reports/state-evolution") && uri.Contains("scope=accounts"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(BuildAccountsEvolutionPayload(payload.AsOf.Year))
                    };
                }

                if (uri.Contains("api/v1/reports/monthly-summary"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new MonthlySummaryDto(
                            new DateOnly(payload.AsOf.Year, payload.AsOf.Month, 1),
                            payload.AsOf.AddDays(1),
                            IncomeTotal: payload.IncomeTotalCents,
                            ExpenseTotal: payload.ExpenseTotalCents,
                            Net: payload.PeriodNetResultCents,
                            TransactionsCount: 3))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        return (httpClientFactory, httpMessageHandlerMock);
    }

    private static Mock<IAccountsApi> BuildAccountsApiMock()
    {
        var mock = new Mock<IAccountsApi>(MockBehavior.Strict);
        mock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    AssetAccountId,
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(2026, 1, 1),
                    false,
                    null),
                new AccountDto(
                    IncomeAccountId,
                    "Salary",
                    AccountNature.Income,
                    AccountKind.IncomeSource,
                    new DateOnly(2026, 1, 1),
                    false,
                    null),
                new AccountDto(
                    ExpenseAccountId,
                    "Groceries",
                    AccountNature.Expense,
                    AccountKind.ExpenseCategory,
                    new DateOnly(2026, 1, 1),
                    false,
                    null)
            ]);

        return mock;
    }

    private static MonthlyEvolutionReportDto BuildAccountsEvolutionPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.Accounts,
            [
                new MonthlyEvolutionSeriesDto(
                    "account:asset-main-bank",
                    "Main Bank",
                    AssetAccountId,
                    "account",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 100_000, 100_000, 100_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 120_000, 20_000, 120_000)
                    ]),
                new MonthlyEvolutionSeriesDto(
                    "account:income-salary",
                    "Salary",
                    IncomeAccountId,
                    "account",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 50_000, 50_000, 50_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 60_000, 10_000, 60_000)
                    ]),
                new MonthlyEvolutionSeriesDto(
                    "account:expense-groceries",
                    "Groceries",
                    ExpenseAccountId,
                    "account",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 30_000, 30_000, 30_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, DateTime.DaysInMonth(year, 2)), 32_000, 2_000, 32_000)
                    ])
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



