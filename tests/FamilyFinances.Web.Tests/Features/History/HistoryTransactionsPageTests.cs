using System.Net;
using System.Net.Http.Json;
using Bunit;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.History;
using FamilyFinances.Web.State;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.History;

public sealed class HistoryTransactionsPageTests : TestContext
{
    [Fact]
    public void RendersReadOnlyHistoryWithoutEditOrDeleteActions()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new List<FiscalYearStatusDto>
                {
                    new(2025, true, DateTime.UtcNow, "admin", null, null)
                })
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new List<TransactionListItemDto>
                {
                    new(
                        Guid.NewGuid(),
                        new DateOnly(2025, 1, 10),
                        "Groceries",
                        "Supermarket",
                        25m,
                        TransactionListItemType.Expense)
                })
            });

        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(x => x.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore();
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddSingleton(httpClientFactory.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddSingleton<AuthenticationStateProvider>(authProvider);
        Services.AddSingleton<HistoryRefreshNotifier>();
        Services.AddScoped<HistoryApi>();

        var cut = RenderComponent<HistoryTransactionsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Groceries");
        });

        cut.Markup.Should().NotContain("Edit");
        cut.Markup.Should().NotContain("Delete");
        cut.Markup.Should().Contain("readonly=true");
        cut.Markup.Should().Contain("returnTo=history-transactions");
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
