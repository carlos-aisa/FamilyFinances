using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
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

public sealed class HistoryTransactionsPageTests : WebTestContext
{
    [Fact]
    public void QueryYear_Is_Applied_To_Filter_And_ReadOnly_Links()
    {
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

                if (uri.Contains("api/v1/history/transactions", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new List<TransactionListItemDto>
                        {
                            new(
                                Guid.NewGuid(),
                                new DateOnly(2024, 12, 10),
                                "Year-end movement",
                                "Salary",
                                "Employer",
                                25m,
                                TransactionListItemType.Income)
                        })
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
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

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo("/history/transactions?year=2024");

        var cut = RenderComponent<HistoryTransactionsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Year-end movement");
            cut.Markup.Should().Contain(MoneyFormatter.FormatEuros(25m));
            cut.Markup.Should().NotContain("$");
        });

        requestedUris.Should().Contain(uri => uri.Contains("api/v1/history/transactions?year=2024&take=1000", StringComparison.OrdinalIgnoreCase));
        cut.Markup.Should().Contain("year=2024");
    }

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
        cut.Markup.Should().Contain("origin=history-transactions");
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
