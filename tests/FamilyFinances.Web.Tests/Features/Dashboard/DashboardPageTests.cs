using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Dashboard;
using FamilyFinances.Web.Features.Reports.Charts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Dashboard;

public sealed class DashboardPageTests : WebTestContext
{
    [Fact]
    public void Dashboard_Renders_Analytics_Blocks_Without_Tabs_Or_Report_Cards()
    {
        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload()));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='dashboard-kpi-strip']");
            cut.Find("[data-testid='dashboard-monthly-income-expense-chart']");
            cut.Find("[data-testid='dashboard-annual-income-expense-chart']");
            cut.Find("[data-testid='dashboard-monthly-net-trend-chart']");
            cut.Find("[data-testid='dashboard-asset-evolution-chart']");
            cut.Find("[data-testid='dashboard-group-annual-evolution-chart']");
            var compositionChart = cut.Find("[data-testid='dashboard-expense-composition-chart']");
            cut.Find("[data-testid='dashboard-open-quick-entry']");
            compositionChart.TextContent.Should().Contain("2026-03");

            cut.Markup.Should().NotContain("ff-premium-tabs");
            cut.Markup.Should().NotContain("report-card");
            cut.Markup.Should().NotContain("dashboard-group-state-chart");
        });
    }

    [Fact]
    public void Dashboard_Expense_Composition_Chart_Has_Total_Percentage_Close_To_OneHundred()
    {
        var compositionRows =
            new[]
            {
                new DashboardCompactInsightRowDto("row-1", "top-expense", "Housing", 60_000, 50m, "top-contributor"),
                new DashboardCompactInsightRowDto("row-2", "top-expense", "Food", 36_000, 30m, "top-contributor"),
                new DashboardCompactInsightRowDto("row-others", "top-expense", "Others", 24_000, 20m, "others")
            };

        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(compactInsights: compositionRows)));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var chart = cut.Find("[data-testid='dashboard-expense-composition-chart']");
            var totalRaw = chart.GetAttribute("data-total-percentage");
            totalRaw.Should().NotBeNullOrWhiteSpace();
            var total = decimal.Parse(totalRaw!, System.Globalization.CultureInfo.InvariantCulture);
            total.Should().BeApproximately(100m, 0.01m);
        });
    }

    [Fact]
    public void Dashboard_Shows_Data_Sufficiency_State_Message_When_Not_Complete()
    {
        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(dataState: DashboardDataSufficiencyState.Partial)));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var note = cut.Find("[data-testid='dashboard-data-sufficiency-state']");
            note.TextContent.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void Dashboard_Chart_Payloads_Use_Shared_Semantic_Palette()
    {
        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload()));
        var lineCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualLineChart", _ => true);
        var barCall = JSInterop.SetupVoid("familyFinancesCharts.renderAnnualBarChart", _ => true);

        RenderComponent<DashboardPage>();

        lineCall.Invocations.Should().NotBeEmpty();
        barCall.Invocations.Should().NotBeEmpty();

        var allColors = new List<string>();
        foreach (var invocation in lineCall.Invocations.Concat(barCall.Invocations))
        {
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(invocation.Arguments[1]);
            using var payload = System.Text.Json.JsonDocument.Parse(payloadJson);
            var datasets = payload.RootElement.GetProperty("datasets");
            allColors.AddRange(datasets.EnumerateArray()
                .Select(dataset => dataset.GetProperty("colorHex").GetString())
                .Where(color => !string.IsNullOrWhiteSpace(color))!
                .Select(color => color!));
        }

        allColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Income));
        allColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Expense));
        allColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Balance));
        allColors.Should().Contain(ChartSemanticPalette.ResolveSemantic(ChartSemanticPalette.Neutral));
    }

    private void RegisterAuthorizedServices(Mock<IHttpClientFactory> httpClientFactory)
    {
        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
    }

    private static Mock<IHttpClientFactory> BuildHttpClientFactory(DashboardOverviewDto payload)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var assetEvolution = CreateAssetEvolutionPayload(payload.AsOf.Year);
        var groupEvolution = CreateGroupEvolutionPayload(payload.AsOf.Year);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                var uri = req.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("api/v1/reports/dashboard-overview", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(payload)
                    });
                }

                if (uri.Contains("api/v1/reports/state-evolution", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains("scope=asset-total", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(assetEvolution)
                    });
                }

                if (uri.Contains("api/v1/reports/state-evolution", StringComparison.OrdinalIgnoreCase) &&
                    uri.Contains("scope=account-groups", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(groupEvolution)
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        return factoryMock;
    }

    private static DashboardOverviewDto CreateOverviewPayload(
        DashboardDataSufficiencyState dataState = DashboardDataSufficiencyState.Complete,
        IReadOnlyList<DashboardCompactInsightRowDto>? compactInsights = null)
    {
        var asOf = new DateOnly(2026, 3, 1);
        return new DashboardOverviewDto(
            AsOf: asOf,
            SelectedMonthStart: new DateOnly(2026, 3, 1),
            SelectedMonthEnd: new DateOnly(2026, 3, 31),
            PreviousMonthStart: new DateOnly(2026, 2, 1),
            PreviousMonthEnd: new DateOnly(2026, 2, 28),
            Income: new DashboardKpiDto(300_000, 50_000),
            Expense: new DashboardKpiDto(120_000, 10_000),
            NetResult: new DashboardKpiDto(180_000, 40_000),
            NetWorth: new DashboardKpiDto(1_250_000, 75_000),
            NetResultDeltaVsSameMonthLastYearCents: 25_000,
            DataSufficiencyState: dataState,
            DailyIncomeVsExpense:
            [
                new DashboardDailyIncomeExpensePointDto(1, 200_000, 50_000, 150_000),
                new DashboardDailyIncomeExpensePointDto(2, 100_000, 70_000, 30_000)
            ],
            GroupStates:
            [
                new DashboardGroupStatePointDto("group:1", "Household", 90_000, 10_000),
                new DashboardGroupStatePointDto("group:2", "Income", -120_000, -15_000)
            ],
            YtdSummary: new DashboardYtdSummaryDto(
                AccumulatedNetCents: 280_000,
                MonthlyNetPoints:
                [
                    new DashboardMonthlyNetPointDto(1, 100_000, 40_000, 60_000, 60_000),
                    new DashboardMonthlyNetPointDto(2, 120_000, 70_000, 50_000, 110_000),
                    new DashboardMonthlyNetPointDto(3, 300_000, 120_000, 180_000, 290_000)
                ]),
            CompactInsights: compactInsights ??
            [
                new DashboardCompactInsightRowDto("a-1", "top-expense", "Groceries", 55_000, 55m, "top-contributor"),
                new DashboardCompactInsightRowDto("a-2", "top-expense", "Utilities", 40_000, 40m, "top-contributor"),
                new DashboardCompactInsightRowDto("a-others", "top-expense", "Others", 5_000, 5m, "others")
            ]);
    }

    private static MonthlyEvolutionReportDto CreateAssetEvolutionPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.AssetTotal,
            [
                new MonthlyEvolutionSeriesDto(
                    "asset-total",
                    "Asset Total",
                    null,
                    "scope",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 2_500_000, 300_000, 300_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, 28), 2_750_000, 250_000, 550_000),
                        new MonthlyEvolutionPointDto(3, new DateOnly(year, 3, 31), 2_820_000, 70_000, 620_000)
                    ])
            ]);
    }

    private static MonthlyEvolutionReportDto CreateGroupEvolutionPayload(int year)
    {
        return new MonthlyEvolutionReportDto(
            year,
            MonthlyEvolutionScope.AccountGroups,
            [
                new MonthlyEvolutionSeriesDto(
                    "group:home",
                    "Home",
                    null,
                    "account-group",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 180_000, 180_000, 180_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, 28), 260_000, 80_000, 260_000),
                        new MonthlyEvolutionPointDto(3, new DateOnly(year, 3, 31), 340_000, 80_000, 340_000)
                    ]),
                new MonthlyEvolutionSeriesDto(
                    "group:utilities",
                    "Utilities",
                    null,
                    "account-group",
                    [
                        new MonthlyEvolutionPointDto(1, new DateOnly(year, 1, 31), 40_000, 40_000, 40_000),
                        new MonthlyEvolutionPointDto(2, new DateOnly(year, 2, 28), 70_000, 30_000, 70_000),
                        new MonthlyEvolutionPointDto(3, new DateOnly(year, 3, 31), 95_000, 25_000, 95_000)
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
