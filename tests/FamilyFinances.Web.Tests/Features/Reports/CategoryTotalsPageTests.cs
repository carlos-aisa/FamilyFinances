using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Reports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class CategoryTotalsPageTests : WebTestContext
{
    [Fact]
    public void CategoryTotals_Supports_Column_Sorting()
    {
        RegisterAuthorizedServices(CreateReportPayload());

        var cut = RenderComponent<CategoryTotalsPage>();
        ClickLoadReport(cut);

        cut.WaitForAssertion(() =>
        {
            cut.FindAll("tbody tr").Should().HaveCount(2);
        });

        GetFirstRowAccountName(cut).Should().Be("Savings");

        cut.FindAll("button.ff-sort-header")[0].Click();
        cut.WaitForAssertion(() => GetFirstRowAccountName(cut).Should().Be("Cash"));

        cut.FindAll("button.ff-sort-header")[0].Click();
        cut.WaitForAssertion(() => GetFirstRowAccountName(cut).Should().Be("Savings"));
    }

    [Fact]
    public async Task CategoryTotals_Row_Click_Drills_Down_To_Account_Movements_With_Context()
    {
        var payload = CreateReportPayload();
        RegisterAuthorizedServices(payload);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<CategoryTotalsPage>();

        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[0].Change("2026-02-01"));
        await cut.InvokeAsync(() => cut.FindAll("input[type='date']")[1].Change("2026-03-01"));

        ClickLoadReport(cut);

        cut.WaitForAssertion(() =>
        {
            cut.FindAll("tbody tr").Should().HaveCount(2);
        });

        cut.FindAll("tbody tr")[0].Click();

        nav.Uri.Should().Contain($"/accounts/{payload.Items[1].AccountId}/movements");
        nav.Uri.Should().Contain("origin=report-category-totals");
        nav.Uri.Should().Contain($"accountId={payload.Items[1].AccountId}");
        nav.Uri.Should().Contain("from=2026-02-01");
        nav.Uri.Should().Contain("to=2026-03-01");
    }

    private void RegisterAuthorizedServices(CategoryTotalsDto payload)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                if (uri.Contains("api/v1/payees", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(Array.Empty<PayeeDto>())
                    };
                }

                if (uri.Contains("api/v1/reports/category-totals", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(payload)
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<ReportsApi>();
        Services.AddScoped<PayeesApi>();
    }

    private static CategoryTotalsDto CreateReportPayload()
    {
        return new CategoryTotalsDto(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 3, 1),
            AccountNature.Expense,
            [
                new CategoryTotalItemDto(
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Cash",
                    100_00,
                    3),
                new CategoryTotalItemDto(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "Savings",
                    300_00,
                    8)
            ]);
    }

    private static string GetFirstRowAccountName(IRenderedComponent<CategoryTotalsPage> cut)
        => cut.FindAll("tbody tr")[0].Children[0].TextContent.Trim();

    private static void ClickLoadReport(IRenderedComponent<CategoryTotalsPage> cut)
    {
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("load", StringComparison.OrdinalIgnoreCase) &&
                             button.TextContent.Contains("report", StringComparison.OrdinalIgnoreCase))
            .Click();
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token)
        {
            _token = token;
        }

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
