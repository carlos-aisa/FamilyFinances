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
}
