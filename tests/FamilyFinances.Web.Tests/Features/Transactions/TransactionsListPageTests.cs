using System.Net;
using System.Net.Http.Json;
using Bunit;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Transactions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Transactions;

public sealed class TransactionsListPageTests : WebTestContext
{
    [Fact]
    public void Transactions_List_Shows_Dedicated_Payee_Column_Without_Description_Duplication()
    {
        RegisterServices(
        [
            new TransactionListItemDto(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                new DateOnly(2026, 2, 15),
                "Groceries",
                "Weekly food",
                "Supermarket X",
                -120.50m,
                TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Payee");
            cut.FindAll("tbody tr").Should().HaveCount(1);
        });

        var firstRowCells = cut.FindAll("tbody tr")[0].Children;
        firstRowCells[2].TextContent.Should().Contain("Weekly food");
        firstRowCells[2].TextContent.Should().NotContain("Supermarket X");
        firstRowCells[3].TextContent.Should().Contain("Supermarket X");
    }

    [Fact]
    public void Transactions_Search_Matches_Payee_Text()
    {
        RegisterServices(
        [
            new TransactionListItemDto(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                new DateOnly(2026, 2, 12),
                "Groceries",
                "Weekly food",
                "Market One",
                -45m,
                TransactionListItemType.Expense),
            new TransactionListItemDto(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                new DateOnly(2026, 2, 13),
                "Salary",
                "Monthly salary",
                "Employer",
                2_000m,
                TransactionListItemType.Income)
        ]);

        var cut = RenderComponent<TransactionsListPage>();

        cut.WaitForAssertion(() =>
        {
            cut.FindAll("tbody tr").Should().HaveCount(2);
        });

        cut.Find("input[placeholder='Description or payee...']").Input("market one");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(1);
            rows[0].TextContent.Should().Contain("Market One");
            rows[0].TextContent.Should().NotContain("Employer");
        });
    }

    private void RegisterServices(IReadOnlyList<TransactionListItemDto> items)
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
                if (req.Method == HttpMethod.Get && uri.Contains("api/v1/transactions?take=1000", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(items)
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
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddScoped<TransactionsApi>();
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
