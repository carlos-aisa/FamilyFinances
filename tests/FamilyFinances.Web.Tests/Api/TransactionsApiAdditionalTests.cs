using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class TransactionsApiAdditionalTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly TransactionsApi _sut;

    public TransactionsApiAdditionalTests()
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

        _sut = new TransactionsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ListAsync_ReturnsPayload_AndSetsBearerHeader()
    {
        HttpRequestMessage? captured = null;
        var payload = new[]
        {
            new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 4),
                "Groceries",
                "Food",
                "Mercadona",
                -54.12m,
                TransactionListItemType.Expense)
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

        var result = await _sut.ListAsync(25, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Headline.Should().Be("Groceries");
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Get);
        captured.RequestUri!.ToString().Should().Contain("api/v1/transactions?take=25");
        captured.Headers.Authorization!.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task ListAsync_ReturnsEmpty_WhenPayloadIsNull()
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

        var result = await _sut.ListAsync(10, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_ThrowsUnauthorizedAccessException_WhenTokenIsMissing()
    {
        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.ListAsync(10, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetLatestExpensesAsync_ReturnsPayload_AndSetsBearerHeader()
    {
        HttpRequestMessage? captured = null;
        var payload = new[]
        {
            new LatestExpenseMovementDto(Guid.NewGuid(), new DateOnly(2026, 2, 4), "Groceries", 5_412)
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
                Content = JsonContent.Create<IReadOnlyList<LatestExpenseMovementDto>>(payload)
            });

        var result = await _sut.GetLatestExpensesAsync(CancellationToken.None);

        result.Should().ContainSingle().Which.AmountCents.Should().Be(5_412);
        captured!.RequestUri!.ToString().Should().Contain("api/v1/transactions/latest-expenses");
        captured.Headers.Authorization!.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPayload_WhenSuccessful()
    {
        var transactionId = Guid.NewGuid();
        var payload = BuildTransaction(transactionId);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.GetByIdAsync(transactionId, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsInvalidOperationException_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var act = () => _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Empty response payload.");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenTransactionNotFound()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var act = () => _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Transaction not found.");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_OnBadRequest_WithRawMessage()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Year is closed.")
            });

        var act = () => _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Year is closed.");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_OnBadRequest_WithApiError()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Put && !request.RequestUri!.ToString().Contains("multi-split", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(new Dictionary<string, string> { ["error"] = "Invalid transaction data." })
            });

        var requestBody = BuildUpdateRequest(Guid.NewGuid());
        var act = () => _sut.UpdateAsync(requestBody.Id, requestBody, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid transaction data.");
    }

    [Fact]
    public async Task UpdateMultiSplitAsync_ThrowsInvalidOperationException_WhenTransactionNotFound()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Put && request.RequestUri!.ToString().Contains("multi-split", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var requestBody = BuildUpdateMultiSplitRequest(Guid.NewGuid());
        var act = () => _sut.UpdateMultiSplitAsync(requestBody.Id, requestBody, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Transaction not found.");
    }

    [Fact]
    public async Task HasAnyAsync_ReturnsFalse_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/transactions/any", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var result = await _sut.HasAnyAsync(CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchExpensesAsync_ReturnsPayload_AndBuildsQueryWithOptionalExpenseAccount()
    {
        HttpRequestMessage? captured = null;
        var expenseAccountId = Guid.NewGuid();
        var payload = new List<ExpenseSearchResultDto>
        {
            new(Guid.NewGuid(), "Coffee", new DateOnly(2026, 2, 2), "Cafe", 2.30m, "Cash")
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
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.SearchExpensesAsync(
            query: "coffee latte",
            expenseAccountId: expenseAccountId,
            limit: 15,
            ct: CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Description.Should().Be("Coffee");
        captured.Should().NotBeNull();
        var uri = captured!.RequestUri!.ToString();
        uri.Should().Contain("api/v1/transactions/search-expenses?");
        uri.Should().Contain("limit=15");
        uri.Should().Contain($"expenseAccountId={expenseAccountId}");
        (uri.Contains("q=coffee%20latte", StringComparison.Ordinal) ||
         uri.Contains("q=coffee latte", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Fact]
    public async Task SearchExpensesAsync_ReturnsEmpty_WhenPayloadIsNull()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("search-expenses", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var result = await _sut.SearchExpensesAsync("rent", null, 5, CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static TransactionDto BuildTransaction(Guid id)
    {
        return new TransactionDto(
            id,
            new DateOnly(2026, 2, 4),
            "Groceries",
            Guid.NewGuid(),
            "Mercadona",
            new[]
            {
                new TransactionSplitDto(Guid.NewGuid(), -54.12m, "Weekly food"),
                new TransactionSplitDto(Guid.NewGuid(), 54.12m, null)
            });
    }

    private static UpdateTransactionRequest BuildUpdateRequest(Guid id)
    {
        return new UpdateTransactionRequest(
            id,
            new DateOnly(2026, 2, 4),
            "Updated description",
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            20m);
    }

    private static UpdateMultiSplitTransactionRequest BuildUpdateMultiSplitRequest(Guid id)
    {
        return new UpdateMultiSplitTransactionRequest(
            id,
            new DateOnly(2026, 2, 4),
            "Split update",
            null,
            new List<TransactionSplitInput>
            {
                new(Guid.NewGuid(), -1000, "Expense"),
                new(Guid.NewGuid(), 1000, "Counterpart")
            });
    }
}
