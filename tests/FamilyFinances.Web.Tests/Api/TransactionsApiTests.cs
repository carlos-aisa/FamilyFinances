using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class TransactionsApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly TransactionsApi _sut;

    public TransactionsApiTests()
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

        _sut = new TransactionsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WithDomainMessage_OnBadRequest()
    {
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Year 2025 is closed. Reopen the year to modify movements." })
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var request = new CreateTransactionRequest(
            new DateOnly(2025, 1, 2),
            "Blocked",
            new List<TransactionSplitInput>
            {
                new(Guid.NewGuid(), -1000, null),
                new(Guid.NewGuid(), 1000, null)
            },
            null);

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");
    }
}
