using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Settings;

public sealed class ApiLanHostOperationsServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<ApiLanHostOperationsService>> _loggerMock = new();

    private readonly HttpContextAccessor _httpContextAccessor = new();

    private ApiLanHostOperationsService CreateSut()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        return new ApiLanHostOperationsService(
            _httpClientFactoryMock.Object,
            _tokenStoreMock.Object,
            _httpContextAccessor,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsStatusAndSendsBearerToken()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns("token-store");

        HttpRequestMessage? capturedRequest = null;
        var expected = new LanAccessStatus(
            Enabled: true,
            HttpsPort: 5443,
            HostName: "familyfinances.local",
            CertificateThumb: "ABC",
            CertificateSubject: "CN=familyfinances.local",
            FirewallRuleName: "FamilyFinances.Web.LAN.HTTPS",
            FirewallEnabled: true);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expected)
            });

        var sut = CreateSut();
        var result = await sut.GetStatusAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Get);
        capturedRequest.RequestUri!.ToString().Should().Contain("api/v1/ops/lan/status");
        capturedRequest.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be("token-store");
    }

    [Fact]
    public async Task GetStatusAsync_UsesCookieToken_WhenStoreIsEmpty()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns((string?)null);

        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "ff_access_token=cookie-token";
        _httpContextAccessor.HttpContext = context;

        HttpRequestMessage? capturedRequest = null;
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new LanAccessStatus(
                    Enabled: false,
                    HttpsPort: 5443,
                    HostName: "host",
                    CertificateThumb: null,
                    CertificateSubject: null,
                    FirewallRuleName: "rule",
                    FirewallEnabled: false))
            });

        var sut = CreateSut();
        await sut.GetStatusAsync(CancellationToken.None);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization!.Parameter.Should().Be("cookie-token");
    }

    [Fact]
    public async Task GetStatusAsync_ThrowsUnauthorizedAccessException_WhenNoTokenIsAvailable()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns((string?)null);
        _tokenStoreMock.Setup(t => t.WaitForTokenAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var sut = CreateSut();
        var act = () => sut.GetStatusAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task ApplyAsync_ReturnsFailure_WhenHttpsPortIsInvalid()
    {
        var sut = CreateSut();
        var result = await sut.ApplyAsync(
            new LanAccessRequest(true, LanAccessCommandValidator.ForbiddenApiPort, "host", false),
            actor: "admin",
            ct: CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Invalid HTTPS port");

        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_ReturnsParsedOperationResult_WhenApiRespondsWithJson()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns("token-store");
        var expectedResult = new LanOperationResult(true, "LAN updated");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.ToString().Contains("/ops/lan/apply")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResult)
            });

        var sut = CreateSut();
        var result = await sut.ApplyAsync(new LanAccessRequest(true, 5443, "host", false), "admin", CancellationToken.None);

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ApplyAsync_ReturnsUnauthorizedResult_WhenApiReturnsUnauthorized()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns("token-store");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var sut = CreateSut();
        var result = await sut.ApplyAsync(new LanAccessRequest(true, 5443, "host", false), "admin", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("API call unauthorized. Missing or invalid token.");
    }

    [Fact]
    public async Task RegenerateCertificateAsync_ReturnsDefaultSuccessMessage_WhenSuccessPayloadIsNotJson()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns("token-store");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri!.ToString().Contains("/certificate/regenerate")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });

        var sut = CreateSut();
        var result = await sut.RegenerateCertificateAsync(5443, "familyfinances.local", "admin", CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be("LAN access state updated.");
    }

    [Fact]
    public async Task ApplyAsync_ReturnsFailureMessageFromPlainText_WhenApiFails()
    {
        _tokenStoreMock.Setup(t => t.GetAccessToken()).Returns("token-store");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("LAN failed due to script error.")
            });

        var sut = CreateSut();
        var result = await sut.ApplyAsync(new LanAccessRequest(true, 5443, "host", false), "admin", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("LAN failed due to script error.");
    }
}
