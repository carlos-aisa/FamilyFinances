using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class HistoryApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly HistoryApi _sut;

    public HistoryApiTests()
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

        _sut = new HistoryApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ListFiscalYearsAsync_ReturnsPayload()
    {
        var payload = new List<FiscalYearStatusDto>
        {
            new(2025, true, DateTime.UtcNow, "admin", null, null),
            new(2026, false, null, null, null, null)
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var result = await _sut.ListFiscalYearsAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Year).Should().Contain(new[] { 2025, 2026 });
    }

    [Fact]
    public async Task CloseYearAsync_ThrowsInvalidOperationException_OnBadRequest()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Year 2025 is already closed." })
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var act = () => _sut.CloseYearAsync(2025, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("already closed");
    }
}
