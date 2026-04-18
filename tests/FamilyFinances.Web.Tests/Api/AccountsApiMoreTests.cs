using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class AccountsApiMoreTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly AccountsApi _sut;

    public AccountsApiMoreTests()
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

        _sut = new AccountsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task GetBalancesAsync_ReturnsPayload_AndAddsBearerHeader()
    {
        HttpRequestMessage? captured = null;
        var payload = new[]
        {
            new AccountBalanceDto(Guid.NewGuid(), 1000m, 200m)
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
                Content = JsonContent.Create<IReadOnlyList<AccountBalanceDto>>(payload)
            });

        var result = await _sut.GetBalancesAsync(CancellationToken.None);

        result.Should().ContainSingle();
        captured.Should().NotBeNull();
        captured!.RequestUri!.ToString().Should().Contain("api/v1/accounts/balances");
        captured.Headers.Authorization!.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task RenameAsync_ThrowsUnauthorizedAccessException_WhenTokenIsMissing()
    {
        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.RenameAsync(Guid.NewGuid(), "Main account", CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task ReopenAsync_SendsPatchRequest_ToReopenEndpoint()
    {
        HttpRequestMessage? captured = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var accountId = Guid.NewGuid();
        await _sut.ReopenAsync(accountId, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Patch);
        captured.RequestUri!.ToString().Should().Contain($"api/v1/accounts/{accountId}/reopen");
    }

    [Fact]
    public async Task ReconcileAsync_ReturnsPayload_WhenSuccessful()
    {
        var payload = new ReconcileAccountResponse(
            AdjustmentCreated: true,
            TransactionId: Guid.NewGuid(),
            ComputedBalance: 1000m,
            ActualBalance: 1200m,
            Difference: 200m,
            Message: "Adjustment created.");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Post && request.RequestUri!.ToString().Contains("/reconcile", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.ReconcileAsync(
            Guid.NewGuid(),
            actualBalance: 1200m,
            asOfDate: new DateOnly(2026, 3, 1),
            note: "check",
            ct: CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task ReconcileAsync_ThrowsKeyNotFoundException_WhenAccountDoesNotExist()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Post && request.RequestUri!.ToString().Contains("/reconcile", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var missingAccountId = Guid.NewGuid();
        var act = () => _sut.ReconcileAsync(
            missingAccountId,
            actualBalance: 100m,
            asOfDate: new DateOnly(2026, 3, 1),
            note: null,
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Account with ID {missingAccountId} not found.");
    }

    [Fact]
    public async Task CloseAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Patch && request.RequestUri!.ToString().Contains("/close", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.CloseAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }
}
