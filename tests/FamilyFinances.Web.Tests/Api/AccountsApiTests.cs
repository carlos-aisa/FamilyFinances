using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FamilyFinances.Web.Tests.Api;

public sealed class AccountsApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IApiTokenStore> _tokenStoreMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly AccountsApi _sut;

    public AccountsApiTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<IApiTokenStore>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

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
    public async Task CreateAsync_ReturnsAccountDto_WhenRequestIsSuccessful()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var expectedDto = new AccountDto(
            Id: Guid.NewGuid(),
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10),
            IsClosed: false,
            ClosedOn: null);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("api/v1/accounts")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedDto.Id);
        result.Name.Should().Be(expectedDto.Name);
        result.Nature.Should().Be(expectedDto.Nature);
        result.Kind.Should().Be(expectedDto.Kind);
        result.OpenedOn.Should().Be(expectedDto.OpenedOn);
        result.IsClosed.Should().BeFalse();
        result.ClosedOn.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ThrowsHttpRequestException_WithErrorMessage_WhenServerReturnsConflict()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Duplicate Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var errorResponse = new { error = "Account name already exists." };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(errorResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.Message.Should().Contain("Account name already exists.");
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateAsync_ThrowsHttpRequestException_WithStatusCode_WhenServerReturnsBadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Invalid Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var errorResponse = new { error = "Invalid account data." };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(errorResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.Message.Should().Contain("Invalid account data.");
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAsync_ThrowsHttpRequestException_WithGenericMessage_WhenServerReturnsNonJsonError()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.Message.Should().Contain("500");
        exception.Which.Message.Should().Contain("InternalServerError");
        exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateAsync_ThrowsUnauthorizedAccessException_WhenNoTokenAvailable()
    {
        // Arrange
        _tokenStoreMock
            .Setup(t => t.GetAccessToken())
            .Returns(string.Empty);

        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task CreateAsync_ThrowsUnauthorizedAccessException_WhenServerReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task CreateAsync_IncludesAuthorizationHeader_WhenTokenIsAvailable()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var expectedDto = new AccountDto(
            Id: Guid.NewGuid(),
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10),
            IsClosed: false,
            ClosedOn: null);

        HttpRequestMessage? capturedRequest = null;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedDto)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("valid-token");
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenResponseIsEmpty()
    {
        // Arrange
        var request = new CreateAccountRequest(
            Name: "Test Account",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 10));

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Empty response payload.");
    }
}
