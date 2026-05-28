using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.History;
using FamilyFinances.Web.Features.Reports;
using FamilyFinances.Web.State;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.History;

public sealed class HistoryMovementsPageTests : WebTestContext
{
    [Fact]
    public void QueryContext_Preserves_Year_And_Account_Selection()
    {
        var selectedAccountId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var counterpartyId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var requestedUris = new List<string>();

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
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                var uri = req.RequestUri!.ToString();
                requestedUris.Add(uri);

                if (uri.Contains("api/v1/fiscal-years", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new List<FiscalYearStatusDto>
                        {
                            new(2025, true, DateTime.UtcNow, "admin", null, null),
                            new(2024, true, DateTime.UtcNow, "admin", null, null)
                        })
                    };
                }

                if (uri.Contains("api/v1/history/movements", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new AccountMovementsDto(
                            selectedAccountId,
                            "Main Bank",
                            new DateOnly(2024, 1, 1),
                            new DateOnly(2025, 1, 1),
                            [
                                new AccountMovementDto(
                                    TransactionId: Guid.Parse("99999999-8888-7777-6666-555555555555"),
                                    BookedOn: new DateOnly(2024, 2, 15),
                                    Description: "Historical payment",
                                    PayeeName: "Utility Co",
                                    SignedAmount: -25m,
                                    CounterpartyAccountName: "Utilities",
                                    RunningBalance: 975m)
                            ],
                            TotalCount: 1))
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    selectedAccountId,
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(2020, 1, 1),
                    false,
                    null),
                new AccountDto(
                    counterpartyId,
                    "Savings",
                    AccountNature.Asset,
                    AccountKind.Savings,
                    new DateOnly(2020, 1, 1),
                    false,
                    null)
            ]);

        var tokenStore = new TestTokenStore();
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddSingleton<AuthenticationStateProvider>(authProvider);
        Services.AddSingleton(accountsApiMock.Object);
        Services.AddSingleton<HistoryRefreshNotifier>();
        Services.AddScoped<HistoryApi>();

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/history/movements?year=2024&accountId={selectedAccountId}");

        var cut = RenderComponent<HistoryMovementsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Historical payment");
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(-25m));
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(975m));
            cut.Markup.Should().NotContain("$");
        });

        requestedUris.Should().Contain(uri =>
            uri.Contains($"api/v1/history/movements?accountId={selectedAccountId}&year=2024", StringComparison.OrdinalIgnoreCase));

        cut.Markup.Should().Contain("origin=history-movements");
        cut.Markup.Should().Contain($"accountId={selectedAccountId}");
        cut.Markup.Should().Contain("year=2024");
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token = "test-token";

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
