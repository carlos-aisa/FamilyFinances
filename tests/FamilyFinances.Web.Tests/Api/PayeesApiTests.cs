using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class PayeesApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly PayeesApi _sut;

    public PayeesApiTests()
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

        _sut = new PayeesApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ListAsync_ReturnsPayload_AndSetsBearerHeader()
    {
        HttpRequestMessage? captured = null;
        var payload = new[]
        {
            new PayeeDto(Guid.NewGuid(), "Mercadona")
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
                Content = JsonContent.Create<IReadOnlyList<PayeeDto>>(payload)
            });

        var result = await _sut.ListAsync(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Mercadona");
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Get);
        captured.RequestUri!.ToString().Should().Contain("api/v1/payees");
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
        captured.Headers.Authorization.Parameter.Should().Be("valid-token");
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

        var result = await _sut.ListAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_ThrowsUnauthorizedAccessException_WhenTokenIsMissing()
    {
        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns(string.Empty);

        var act = () => _sut.ListAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task CreateAsync_ReturnsPayload_WhenSuccessful()
    {
        var payload = new PayeeDto(Guid.NewGuid(), "Rent");
        var requestBody = new CreatePayeeRequest("Rent");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.CreateAsync(requestBody, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_OnConflict_WithApiMessage()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new Dictionary<string, string>
                {
                    ["error"] = "A payee with this name already exists."
                })
            });

        var act = () => _sut.CreateAsync(new CreatePayeeRequest("Rent"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A payee with this name already exists.");
    }

    [Fact]
    public async Task RenameAsync_ThrowsInvalidOperationException_OnBadRequest_WithRawMessage()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Payee name is required.")
            });

        var act = () => _sut.RenameAsync(Guid.NewGuid(), string.Empty, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payee name is required.");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_OnConflict()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new Dictionary<string, string>
                {
                    ["error"] = "Cannot delete payee with assigned transactions."
                })
            });

        var act = () => _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete payee with assigned transactions.");
    }
}
