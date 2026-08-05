using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class EconomicStateAsOfDatePageTests : WebTestContext
{
    [Fact]
    public void Page_Defaults_To_Today_Limits_Future_Dates_And_Loads_Immediately()
    {
        using var _ = UseCulture("en-US");
        var today = DateOnly.FromDateTime(DateTime.Today);
        var requestedUris = new List<string>();

        var cut = RenderAuthorizedPage(requestedUris: requestedUris);

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains($"asOf={today:yyyy-MM-dd}"));
            var dateInput = cut.Find("[data-testid='economic-state-as-of-date-input']");
            dateInput.GetAttribute("max").Should().Be($"{today:yyyy-MM-dd}");
            dateInput.GetAttribute("value").Should().Be($"{today:yyyy-MM-dd}");
        });
    }

    [Fact]
    public void Page_Applies_Historical_Date_And_Renders_Six_Metrics_With_Explicit_Contexts()
    {
        using var _ = UseCulture("en-US");
        var requestedUris = new List<string>();
        var selectedDate = new DateOnly(2026, 2, 15);

        var cut = RenderAuthorizedPage(requestedUris: requestedUris);
        cut.WaitForAssertion(() => cut.Find("[data-testid='economic-state-as-of-date-input']"));

        cut.Find("[data-testid='economic-state-as-of-date-input']").Change("2026-02-15");
        cut.Find("[data-testid='economic-state-as-of-date-load']").Click();

        cut.WaitForAssertion(() =>
        {
            requestedUris.Should().Contain(uri => uri.Contains($"asOf={selectedDate:yyyy-MM-dd}"));
            cut.Find("[data-testid='economic-state-as-of-date-balance-context']").TextContent
                .Should().Contain("Balance as of: 15-02-2026");
            cut.Find("[data-testid='economic-state-as-of-date-flow-context']").TextContent
                .Should().Contain("Period: 01-02-2026 to 15-02-2026");
            cut.Markup.Should().Contain("Asset balance");
            cut.Markup.Should().Contain("Liability balance");
            cut.Markup.Should().Contain("Net worth");
            cut.Markup.Should().Contain("Income");
            cut.Markup.Should().Contain("Expense");
            cut.Markup.Should().Contain("Period net result");
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(320_000));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(-150_000));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(170_000));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(100_000));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(-30_000));
            cut.Markup.Should().Contain(MoneyFormatter.FormatCentsWithSign(70_000));
            cut.FindAll(".nav-tabs").Should().BeEmpty();
            cut.FindAll("canvas").Should().BeEmpty();
        });
    }

    [Fact]
    public void Page_Shows_Loading_State_While_Request_Is_Pending()
    {
        using var _ = UseCulture("en-US");
        var pendingResponse = new TaskCompletionSource<HttpResponseMessage>();

        var cut = RenderAuthorizedPage(responseFactory: _ => pendingResponse.Task);

        cut.Find("[data-testid='economic-state-as-of-date-loading']");
        pendingResponse.SetResult(CreateSuccessResponse(DateOnly.FromDateTime(DateTime.Today)));
        cut.WaitForAssertion(() => cut.FindAll("[data-testid='economic-state-as-of-date-loading']").Should().BeEmpty());
    }

    [Fact]
    public void Page_Shows_Error_When_Report_Load_Fails()
    {
        using var _ = UseCulture("en-US");
        var cut = RenderAuthorizedPage(responseFactory: _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='economic-state-as-of-date-error']").TextContent
                .Should().Contain("Could not load report data");
        });
    }

    private IRenderedComponent<EconomicStateAsOfDatePage> RenderAuthorizedPage(
        List<string>? requestedUris = null,
        Func<HttpRequestMessage, Task<HttpResponseMessage>>? responseFactory = null)
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns((HttpRequestMessage request, CancellationToken _) =>
            {
                requestedUris?.Add(request.RequestUri!.ToString());

                if (responseFactory is not null)
                    return responseFactory(request);

                var asOf = DateOnly.Parse(request.RequestUri!.Query.Split('=').Last());
                return Task.FromResult(CreateSuccessResponse(asOf));
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(factory => factory.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");
        Services.AddSingleton(httpClientFactory.Object);
        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();

        return RenderComponent<EconomicStateAsOfDatePage>();
    }

    private static HttpResponseMessage CreateSuccessResponse(DateOnly asOf)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new EconomicStateDto(
                asOf,
                AssetsTotalCents: 320_000,
                LiabilitiesTotalCents: -150_000,
                NetWorthCents: 170_000,
                IncomeTotalCents: 100_000,
                ExpenseTotalCents: -30_000,
                PeriodNetResultCents: 70_000))
        };
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token) => _token = token;

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
