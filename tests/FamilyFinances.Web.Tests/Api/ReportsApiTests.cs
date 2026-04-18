using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class ReportsApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly ReportsApi _sut;

    public ReportsApiTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns("valid-token");

        _sut = new ReportsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task GetAssetTotalBalanceAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var expectedAsOf = new DateOnly(2026, 1, 31);
        var payload = new AssetTotalBalanceDto(expectedAsOf, 123_456, 3);
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetAssetTotalBalanceAsync(expectedAsOf, CancellationToken.None);

        result.AsOf.Should().Be(expectedAsOf);
        result.TotalCents.Should().Be(123_456);
        result.AssetAccountsCount.Should().Be(3);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString().Should().Contain("api/v1/reports/asset-total-balance?asOf=2026-01-31");
        capturedRequest.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task GetAssetTotalBalanceAsync_ThrowsUnauthorizedAccessException_WhenNoTokenAvailable()
    {
        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.GetAssetTotalBalanceAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetAssetTotalBalanceAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.GetAssetTotalBalanceAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task GetAssetTotalBalanceAsync_ThrowsInvalidOperationException_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var act = () => _sut.GetAssetTotalBalanceAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize asset total balance response.");
    }

    [Fact]
    public async Task GetEconomicStateAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var expectedAsOf = new DateOnly(2026, 1, 31);
        var payload = new EconomicStateDto(
            AsOf: expectedAsOf,
            AssetsTotalCents: 320_000,
            LiabilitiesTotalCents: 150_000,
            NetWorthCents: 170_000,
            IncomeTotalCents: 100_000,
            ExpenseTotalCents: -30_000,
            PeriodNetResultCents: 70_000);
        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetEconomicStateAsync(expectedAsOf, CancellationToken.None);

        result.AsOf.Should().Be(expectedAsOf);
        result.AssetsTotalCents.Should().Be(320_000);
        result.LiabilitiesTotalCents.Should().Be(150_000);
        result.NetWorthCents.Should().Be(170_000);
        result.IncomeTotalCents.Should().Be(100_000);
        result.ExpenseTotalCents.Should().Be(-30_000);
        result.PeriodNetResultCents.Should().Be(70_000);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString().Should().Contain("api/v1/reports/economic-state?asOf=2026-01-31");
        capturedRequest.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task GetEconomicStateAsync_ThrowsUnauthorizedAccessException_WhenNoTokenAvailable()
    {
        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.GetEconomicStateAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetEconomicStateAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.GetEconomicStateAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task GetEconomicStateAsync_ThrowsInvalidOperationException_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var act = () => _sut.GetEconomicStateAsync(new DateOnly(2026, 1, 31), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize economic state response.");
    }

    [Fact]
    public async Task GetDashboardOverviewAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var payload = new DashboardOverviewDto(
            AsOf: new DateOnly(2026, 3, 31),
            SelectedMonthStart: new DateOnly(2026, 3, 1),
            SelectedMonthEnd: new DateOnly(2026, 3, 31),
            PreviousMonthStart: new DateOnly(2026, 2, 1),
            PreviousMonthEnd: new DateOnly(2026, 2, 28),
            Income: new DashboardKpiDto(200_000, 10_000),
            Expense: new DashboardKpiDto(100_000, 5_000),
            NetResult: new DashboardKpiDto(100_000, 5_000),
            NetWorth: new DashboardKpiDto(900_000, 20_000),
            AssetTotal: new DashboardKpiDto(1_000_000, 25_000),
            NetResultDeltaVsSameMonthLastYearCents: 15_000,
            DataSufficiencyState: DashboardDataSufficiencyState.Partial,
            DailyIncomeVsExpense:
            [
                new DashboardDailyIncomeExpensePointDto(1, 120_000, 40_000, 80_000)
            ],
            GroupStates:
            [
                new DashboardGroupStatePointDto("group:1", "Household", 40_000, 10_000)
            ],
            YtdSummary: new DashboardYtdSummaryDto(
                100_000,
                [
                    new DashboardMonthlyNetPointDto(1, 100_000, 20_000, 80_000, 80_000)
                ]),
            CompactInsights:
            [
                new DashboardCompactInsightRowDto("row-1", "top-expense", "Groceries", 20_000, 40m, "top-contributor")
            ]);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetDashboardOverviewAsync(2026, 3, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/dashboard-overview?year=2026&month=3");
        capturedRequest.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task GetMonthlyEvolutionAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var payload = new MonthlyEvolutionReportDto(
            2026,
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
                        new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 12_000, 2_000, 2_000)
                    })
            });

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetMonthlyEvolutionAsync(2026, MonthlyEvolutionScope.AssetTotal, CancellationToken.None);

        result.Year.Should().Be(2026);
        result.Scope.Should().Be(MonthlyEvolutionScope.AssetTotal);
        result.Series.Should().ContainSingle();

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/state-evolution?year=2026&scope=asset-total");
        capturedRequest.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task GetMonthlyEvolutionAsync_ThrowsUnauthorizedAccessException_WhenNoTokenAvailable()
    {
        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.GetMonthlyEvolutionAsync(2026, MonthlyEvolutionScope.Accounts, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetMonthlyEvolutionAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.GetMonthlyEvolutionAsync(2026, MonthlyEvolutionScope.AccountGroups, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task GetMonthlyEvolutionAsync_ThrowsInvalidOperationException_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var act = () => _sut.GetMonthlyEvolutionAsync(2026, MonthlyEvolutionScope.AssetTotal, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize monthly evolution response.");
    }

    [Fact]
    public async Task GetStateEvolutionAsync_Maps_ExpenseTotal_To_ExpenseTotal_Scope_Query()
    {
        var payload = new MonthlyEvolutionReportDto(
            2026,
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
                        new MonthlyEvolutionPointDto(1, new DateOnly(2026, 1, 31), 20_000, 20_000, 20_000)
                    })
            });

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetStateEvolutionAsync(2026, MonthlyEvolutionScope.ExpenseTotal, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/state-evolution?year=2026&scope=expense-total");
    }

    [Fact]
    public async Task GetMonthlyChartBalanceAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var accountId = Guid.NewGuid();
        var payload = new MonthlyBalanceChartDto(
            2026,
            2,
            [new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 10_000)]);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetMonthlyChartBalanceAsync(2026, 2, accountId, ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString()
            .Should().Contain($"api/v1/reports/monthly-charts/balance?year=2026&month=2&accountId={accountId}");
    }

    [Fact]
    public async Task GetMonthlyChartBalanceAsync_Appends_Payee_And_Nature_Query_Parameters()
    {
        var payeeId = Guid.NewGuid();
        var payload = new MonthlyBalanceChartDto(
            2026,
            2,
            [new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 10_000)]);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetMonthlyChartBalanceAsync(
            2026,
            2,
            payeeId: payeeId,
            nature: FamilyFinances.Domain.Ledger.Accounts.AccountNature.Income,
            ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString()
            .Should().Contain($"api/v1/reports/monthly-charts/balance?year=2026&month=2&payeeId={payeeId}&nature=Income");
    }

    [Fact]
    public async Task GetMonthlyChartGroupEvolutionAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var payload = new MonthlyBalanceVsGroupsChartDto(
            2026,
            2,
            [
                new MonthlyChartSeriesDto(
                    "asset-total",
                    "Asset Total",
                    null,
                    "scope",
                    [new MonthlyChartPointDto(1, new DateOnly(2026, 2, 1), 10_000)])
            ]);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetMonthlyChartGroupEvolutionAsync(2026, 2, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/monthly-charts/group-evolution?year=2026&month=2");
    }

    [Fact]
    public async Task GetMonthlyChartBalanceAsync_ThrowsUnauthorizedAccessException_WhenNoTokenAvailable()
    {
        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.GetMonthlyChartBalanceAsync(2026, 2, accountId: null, ct: CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetMonthlyChartGroupEvolutionAsync_ThrowsInvalidOperationException_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var act = () => _sut.GetMonthlyChartGroupEvolutionAsync(2026, 2, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize monthly balance vs groups chart response.");
    }

    [Fact]
    public async Task GetParetoInsightsAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var payload = new ReportingParetoInsightsDto(
            From: new DateOnly(2026, 1, 1),
            To: new DateOnly(2026, 2, 1),
            Dimension: ReportingInsightDimension.Group,
            Expense: new ParetoInsightSectionDto(
                FamilyFinances.Domain.Ledger.Accounts.AccountNature.Expense,
                100_000,
                5,
                85_000,
                85m,
                [new ParetoContributorDto(Guid.NewGuid(), "Food", 50_000, 50m)]),
            Income: new ParetoInsightSectionDto(
                FamilyFinances.Domain.Ledger.Accounts.AccountNature.Income,
                120_000,
                5,
                120_000,
                100m,
                [new ParetoContributorDto(Guid.NewGuid(), "Salary", 120_000, 100m)]));

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetParetoInsightsAsync(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 1),
            ReportingInsightDimension.Group,
            topN: 5,
            ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/insights/pareto?from=2026-01-01&to=2026-02-01&dimension=group&topN=5");
    }

    [Fact]
    public async Task GetAnomalyInsightsAsync_ReturnsPayload_And_SendsExpectedRequest()
    {
        var payload = new ReportingAnomalyInsightsDto(
            Year: 2026,
            Month: 2,
            Nature: FamilyFinances.Domain.Ledger.Accounts.AccountNature.Expense,
            Dimension: ReportingInsightDimension.Payee,
            RequiredHistoryMonths: 3,
            ThresholdRule: "rule",
            Contributors:
            [
                new AnomalyContributorDto(
                    Guid.NewGuid(),
                    "Mercadona",
                    40_000,
                    12_000,
                    25_000,
                    3.2m,
                    true,
                    false,
                    6,
                    "threshold exceeded")
            ]);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetAnomalyInsightsAsync(
            2026,
            2,
            FamilyFinances.Domain.Ledger.Accounts.AccountNature.Expense,
            ReportingInsightDimension.Payee,
            lookbackMonths: 12,
            requiredHistoryMonths: 3,
            ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString()
            .Should().Contain("api/v1/reports/insights/anomalies?year=2026&month=2&nature=Expense&dimension=payee&lookbackMonths=12&requiredHistoryMonths=3");
    }
}
