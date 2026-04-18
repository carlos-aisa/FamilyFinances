using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class HistoryApiAdditionalTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly HistoryApi _sut;

    public HistoryApiAdditionalTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns("valid-token");

        _sut = new HistoryApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ReopenYearAsync_ReturnsPayload_WhenSuccessful()
    {
        var payload = new FiscalYearStatusDto(
            2025,
            false,
            DateTime.UtcNow.AddMonths(-1),
            "admin",
            DateTime.UtcNow,
            "admin");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/reopen", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.ReopenYearAsync(2025, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task ReopenYearAsync_ThrowsInvalidOperationException_OnBadRequest_WithRawError()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/reopen", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Cannot reopen future fiscal year.")
            });

        var act = () => _sut.ReopenYearAsync(2030, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot reopen future fiscal year.");
    }

    [Fact]
    public async Task ListHistoricalTransactionsAsync_ClampsTakeToDefault_WhenLowerThanOne()
    {
        HttpRequestMessage? captured = null;
        var payload = new[]
        {
            new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 1),
                "Salary",
                null,
                "Employer",
                1200m,
                TransactionListItemType.Income)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<IReadOnlyList<TransactionListItemDto>>(payload)
            });

        var result = await _sut.ListHistoricalTransactionsAsync(2026, take: 0, CancellationToken.None);

        result.Should().ContainSingle();
        captured.Should().NotBeNull();
        captured!.RequestUri!.ToString().Should().Contain("api/v1/history/transactions?year=2026&take=200");
    }

    [Fact]
    public async Task GetHistoricalMovementsAsync_BuildsExpectedQuery_AndClampsPagination()
    {
        HttpRequestMessage? captured = null;
        var accountId = Guid.NewGuid();
        var payload = new AccountMovementsDto(
            accountId,
            "Checking",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 1),
            [],
            0);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetHistoricalMovementsAsync(
            accountId,
            year: 2026,
            searchQuery: "rent home",
            page: 0,
            pageSize: 999,
            ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        captured.Should().NotBeNull();
        var uri = captured!.RequestUri!.ToString();
        uri.Should().Contain($"accountId={accountId}");
        uri.Should().Contain("year=2026");
        uri.Should().Contain("page=1");
        uri.Should().Contain("pageSize=100");
        (uri.Contains("q=rent%20home", StringComparison.Ordinal) ||
         uri.Contains("q=rent home", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public async Task GetHistoricalMovementsAsync_ThrowsKeyNotFoundException_OnNotFound()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/history/movements", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = JsonContent.Create(new Dictionary<string, string>
                {
                    ["error"] = "Account does not exist."
                })
            });

        var act = () => _sut.GetHistoricalMovementsAsync(
            Guid.NewGuid(),
            year: 2026,
            searchQuery: null,
            page: 1,
            pageSize: 50,
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Account does not exist.");
    }

    [Fact]
    public async Task ListFiscalYearsAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.ListFiscalYearsAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task CloseYearAsync_ThrowsUnauthorizedAccessException_WhenTokenIsMissing()
    {
        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.CloseYearAsync(2026, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }
}
