using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Dashboard;
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
            cut.Find("[data-testid='dashboard-ytd-summary']");
            cut.Find("[data-testid='dashboard-group-state-chart']");
            cut.Find("[data-testid='dashboard-compact-insights']");
            cut.Find("[data-testid='dashboard-open-quick-entry']");

            cut.Markup.Should().NotContain("ff-premium-tabs");
            cut.Markup.Should().NotContain("report-card");
        });
    }

    [Fact]
    public void Dashboard_Compact_Insights_Is_Row_Capped_At_Eight()
    {
        var oversizedInsights = Enumerable.Range(1, 12)
            .Select(index => new DashboardCompactInsightRowDto(
                RowKey: $"row-{index}",
                Kind: "top-expense",
                Label: $"Item {index}",
                AmountCents: 10_000 + index,
                Percentage: 10m + index,
                StatusCode: "top-contributor"))
            .ToList();

        RegisterAuthorizedServices(BuildHttpClientFactory(CreateOverviewPayload(compactInsights: oversizedInsights)));

        var cut = RenderComponent<DashboardPage>();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("[data-testid='dashboard-compact-insights'] tbody tr");
            rows.Should().HaveCount(8);
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

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("api/v1/reports/dashboard-overview", StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
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
                new DashboardCompactInsightRowDto("a-1", "anomaly", "Groceries", 55_000, null, "anomaly"),
                new DashboardCompactInsightRowDto("a-2", "top-expense", "Utilities", 40_000, 20m, "top-contributor")
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
