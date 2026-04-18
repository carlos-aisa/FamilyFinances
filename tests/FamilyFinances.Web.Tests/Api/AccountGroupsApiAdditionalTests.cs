using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class AccountGroupsApiAdditionalTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly AccountGroupsApi _sut;

    public AccountGroupsApiAdditionalTests()
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

        _sut = new AccountGroupsApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ReturnsPayload_WhenSuccessful()
    {
        var payload = new AccountGroupDto(Guid.NewGuid(), "Household", "Main expenses");

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

        var result = await _sut.CreateAsync(new CreateAccountGroupRequest("Household", "Main expenses"), CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_OnBadRequest_WithRawMessage()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Name is required.")
            });

        var act = () => _sut.CreateAsync(new CreateAccountGroupRequest("", null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Name is required.");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPayload_WhenSuccessful()
    {
        var groupId = Guid.NewGuid();
        var payload = new AccountGroupDetailsDto(
            groupId,
            "Household",
            "Main expenses",
            [
                new AccountRefDto(Guid.NewGuid(), "Checking", AccountNature.Asset, AccountKind.Checking)
            ]);

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

        var result = await _sut.GetByIdAsync(groupId, CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public async Task RenameAsync_ThrowsKeyNotFoundException_WhenGroupDoesNotExist()
    {
        var groupId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Patch && request.RequestUri!.ToString().Contains("/rename", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var act = () => _sut.RenameAsync(groupId, "New Name", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Account group with ID {groupId} not found.");
    }

    [Fact]
    public async Task RemoveAccountAsync_SendsDeleteRequest_ToExpectedRoute()
    {
        HttpRequestMessage? captured = null;
        var groupId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        await _sut.RemoveAccountAsync(groupId, accountId, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Delete);
        captured.RequestUri!.ToString().Should().Contain($"api/v1/account-groups/{groupId}/accounts/{accountId}");
    }

    [Fact]
    public async Task RemoveAccountAsync_ThrowsUnauthorizedAccessException_WhenApiReturnsUnauthorized()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Delete && request.RequestUri!.ToString().Contains("/accounts/", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var act = () => _sut.RemoveAccountAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("API call unauthorized. Missing or invalid token.");
    }
}
