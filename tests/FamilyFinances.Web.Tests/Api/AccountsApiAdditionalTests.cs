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

public sealed class AccountsApiAdditionalTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly AccountsApi _sut;

    public AccountsApiAdditionalTests()
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

        _sut = new AccountsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ListAsync_ReturnsItems_AndSendsAuthorizationHeader()
    {
        HttpRequestMessage? capturedRequest = null;
        var payload = new[]
        {
            new AccountDto(
                Guid.NewGuid(),
                "Checking",
                FamilyFinances.Domain.Ledger.Accounts.AccountNature.Asset,
                FamilyFinances.Domain.Ledger.Accounts.AccountKind.Checking,
                new DateOnly(2026, 1, 1),
                false,
                null)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<IReadOnlyList<AccountDto>>(payload)
            });

        var result = await _sut.ListAsync(CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Checking");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString().Should().Contain("api/v1/accounts");
        capturedRequest.Headers.Authorization!.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyCollection_WhenPayloadIsNull()
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

        var result = await _sut.ListAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMovementsAsync_BuildsExpectedQueryParameters()
    {
        var accountId = Guid.NewGuid();
        HttpRequestMessage? capturedRequest = null;
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
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetMovementsAsync(
            accountId,
            fromInclusive: new DateOnly(2026, 1, 1),
            toExclusive: new DateOnly(2026, 2, 1),
            searchQuery: "rent home",
            minAmount: 10.5m,
            maxAmount: 2000.75m,
            page: 2,
            pageSize: 25,
            ct: CancellationToken.None);

        result.AccountId.Should().Be(accountId);
        capturedRequest.Should().NotBeNull();
        var url = capturedRequest!.RequestUri!.ToString();
        url.Should().Contain($"api/v1/accounts/{accountId}/movements");
        url.Should().Contain("from=2026-01-01");
        url.Should().Contain("to=2026-02-01");
        (url.Contains("q=rent%20home", StringComparison.Ordinal) ||
         url.Contains("q=rent home", StringComparison.Ordinal)).Should().BeTrue();
        url.Should().Contain("minAmount=10.5");
        url.Should().Contain("maxAmount=2000.75");
        url.Should().Contain("page=2");
        url.Should().Contain("pageSize=25");
    }

    [Fact]
    public async Task GetMovementsAsync_OmitsAmountQueryParameters_WhenNotProvided()
    {
        var accountId = Guid.NewGuid();
        HttpRequestMessage? capturedRequest = null;
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
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        _ = await _sut.GetMovementsAsync(
            accountId,
            fromInclusive: new DateOnly(2026, 1, 1),
            toExclusive: new DateOnly(2026, 2, 1),
            searchQuery: "rent home",
            page: 1,
            pageSize: 50,
            ct: CancellationToken.None);

        capturedRequest.Should().NotBeNull();
        var url = capturedRequest!.RequestUri!.ToString();
        url.Should().NotContain("minAmount=");
        url.Should().NotContain("maxAmount=");
    }

    [Fact]
    public async Task GetMovementsAsync_ThrowsKeyNotFoundException_WhenAccountDoesNotExist()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var act = () => _sut.GetMovementsAsync(Guid.NewGuid(), ct: CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ReconcileAsync_ThrowsInvalidOperationException_OnBadRequest()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(new Dictionary<string, string>
                {
                    ["error"] = "Actual balance is required."
                })
            });

        var act = () => _sut.ReconcileAsync(
            Guid.NewGuid(),
            actualBalance: 100m,
            asOfDate: new DateOnly(2026, 2, 10),
            note: null,
            ct: CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Actual balance is required.");
    }

    [Fact]
    public async Task CloseAsync_ThrowsInvalidOperationException_OnConflict()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch && r.RequestUri!.ToString().Contains("/close")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new Dictionary<string, string>
                {
                    ["error"] = "Account is already closed."
                })
            });

        var act = () => _sut.CloseAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Account is already closed.");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_OnConflictWithRawPayload()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("Cannot delete account with linked transactions.")
            });

        var act = () => _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete account with linked transactions.");
    }
}
