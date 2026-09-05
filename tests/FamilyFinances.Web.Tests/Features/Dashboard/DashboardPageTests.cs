using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Dashboard;
using FamilyFinances.Web.Features.Reports;
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
            cut.Find("[data-testid='dashboard-asset-evolution-chart']");
            cut.Find("[data-testid='dashboard-expense-kind-ranking']");
            cut.Find("[data-testid='dashboard-pinned-groups']");
            cut.Find("[data-testid='dashboard-monthly-summary']");
            cut.Find("[data-testid='dashboard-open-quick-entry']");

            var secondRow = cut.Find(".ff-dashboard-analytics-row-2");
            secondRow.QuerySelectorAll(":scope > div").Should().HaveCount(3);
            secondRow.QuerySelectorAll(":scope > div").Should().OnlyContain(column =>
                column.ClassList.Contains("col-12") && column.ClassList.Contains("col-xxl-4"));
            secondRow.QuerySelector("[data-testid='dashboard-monthly-income-expense-chart']").Should().NotBeNull();
            secondRow.QuerySelector("[data-testid='dashboard-annual-income-expense-chart']").Should().NotBeNull();
            secondRow.QuerySelector("[data-testid='dashboard-asset-evolution-chart']").Should().NotBeNull();

            var thirdRow = cut.Find(".ff-dashboard-analytics-row-3");
            thirdRow.QuerySelectorAll(":scope > div").Should().HaveCount(3);
            thirdRow.QuerySelectorAll(":scope > div").Should().OnlyContain(column =>
                column.ClassList.Contains("col-12") && column.ClassList.Contains("col-xxl-4"));
            thirdRow.QuerySelector("[data-testid='dashboard-expense-kind-ranking']").Should().NotBeNull();
            thirdRow.QuerySelector("[data-testid='dashboard-pinned-groups']").Should().NotBeNull();
            thirdRow.QuerySelector("[data-testid='dashboard-monthly-summary']").Should().NotBeNull();
            cut.FindAll("[data-testid='dashboard-monthly-summary-insight']").Count.Should().BeLessThanOrEqualTo(4);

            cut.Markup.Should().NotContain("ff-premium-tabs");
            cut.Markup.Should().NotContain("report-card");
            cut.Markup.Should().NotContain("dashboard-group-state-chart");
        });
    }

    [Fact]
    public void Dashboard_Expense_Kind_Ranking_Renders_TopRows()
    {
        var rankingRows =
            new[]
            {
                new DashboardExpenseKindRankDto(Guid.NewGuid(), "Housing", 60_000, 50m, false),
                new DashboardExpenseKindRankDto(Guid.NewGuid(), "Food", 36_000, 30m, false),
                new DashboardExpenseKindRankDto(null, "Others", 24_000, 20m, true)
            };

        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(expenseKindRanking: rankingRows)));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var ranking = cut.Find("[data-testid='dashboard-expense-kind-ranking']");
            ranking.TextContent.Should().Contain("Housing").And.Contain("Food").And.Contain("Others");
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

    [Fact]
    public void Dashboard_Displays_Annual_Accumulation_KPI()
    {
        using var culture = UseCulture("es-ES");
        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(), previousYearAssetTotalCents: 1_200_000));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var kpiStrip = cut.Find("[data-testid='dashboard-kpi-strip']");
            var kpiCards = kpiStrip.QuerySelectorAll(".card");
            
            kpiCards.Should().HaveCount(5, "dashboard should display 5 KPI cards");
            
            var fifthKpi = kpiCards[4];
            fifthKpi.ClassList.Should().Contain("border-warning", "fifth KPI should have yellow/orange border");
            
            var label = fifthKpi.QuerySelector("h6");
            label.Should().NotBeNull();
            label!.TextContent.Should().Contain("Acum. anual", "label should use the family-oriented Spanish terminology");
            
            var value = fifthKpi.QuerySelector("h4");
            value.Should().NotBeNull();
            value!.TextContent.Should().Contain(
                MoneyFormatter.FormatCentsWithSign(300_000),
                "YTD value should be current assets minus asset total at 31/12 of previous year");
            
            var delta = fifthKpi.QuerySelector("small");
            delta.Should().NotBeNull();
            delta!.TextContent.Should().NotBeNullOrWhiteSpace("delta should be displayed");
        });
    }

    [Fact]
    public void Dashboard_PinnedGroups_Uses_Annual_Accumulation_Header()
    {
        using var culture = UseCulture("es-ES");
        var pinnedGroups =
            new[]
            {
                new DashboardPinnedGroupOperationalResultDto(Guid.NewGuid(), "Household", 15_000, 45_000)
            };
        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(pinnedGroups: pinnedGroups)));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var pinnedGroupsTable = cut.Find("[data-testid='dashboard-pinned-groups']");
            pinnedGroupsTable.TextContent.Should().Contain("Acum. anual");
        });
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

    private static Mock<IHttpClientFactory> BuildHttpClientFactory(
        DashboardOverviewDto payload,
        long previousYearAssetTotalCents = 1_220_000)
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

                if (uri.Contains("api/v1/reports/asset-total-balance", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new AssetTotalBalanceDto(
                            AsOf: new DateOnly(payload.AsOf.Year - 1, 12, 31),
                            TotalCents: previousYearAssetTotalCents,
                            AssetAccountsCount: 2))
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
        IReadOnlyList<DashboardCompactInsightRowDto>? compactInsights = null,
        IReadOnlyList<DashboardExpenseKindRankDto>? expenseKindRanking = null,
        IReadOnlyList<DashboardPinnedGroupOperationalResultDto>? pinnedGroups = null)
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
            AssetTotal: new DashboardKpiDto(1_500_000, 80_000),
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
            ],
            ExpenseKindRanking: expenseKindRanking ??
            [
                new DashboardExpenseKindRankDto(Guid.NewGuid(), "Groceries", 55_000, 55m, false),
                new DashboardExpenseKindRankDto(null, "Others", 45_000, 45m, true)
            ],
            PinnedGroups: pinnedGroups ?? Array.Empty<DashboardPinnedGroupOperationalResultDto>());
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
