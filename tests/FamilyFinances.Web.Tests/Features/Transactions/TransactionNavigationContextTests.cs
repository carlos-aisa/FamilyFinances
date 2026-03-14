using System.Net;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Transactions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Transactions;

public sealed class TransactionNavigationContextTests : WebTestContext
{
    [Fact]
    public void Account_Origin_Preserves_Context_In_Back_And_Edit_Links()
    {
        var transactionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        RegisterServices(transactionId, accountId);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/transactions/{transactionId}?origin=accounts-movements&accountId={accountId}&from=2026-02-01&to=2026-02-28");

        var cut = RenderComponent<TransactionDetailPage>(parameters => parameters.Add(x => x.Id, transactionId));

        cut.WaitForAssertion(() =>
        {
            cut.Find("a.btn.btn-outline-secondary");
            cut.Find("a.btn.btn-primary");
        });

        var backHref = cut.Find("a.btn.btn-outline-secondary").GetAttribute("href");
        var editHref = cut.Find("a.btn.btn-primary").GetAttribute("href");

        backHref.Should().Contain($"/accounts/{accountId}/movements");
        backHref.Should().Contain("origin=accounts-movements");
        backHref.Should().Contain($"accountId={accountId}");
        backHref.Should().Contain("from=2026-02-01");
        backHref.Should().Contain("to=2026-02-28");

        editHref.Should().Contain($"/transactions/{transactionId}/edit");
        editHref.Should().Contain("origin=accounts-movements");
        editHref.Should().Contain($"accountId={accountId}");
        editHref.Should().Contain("from=2026-02-01");
        editHref.Should().Contain("to=2026-02-28");
    }

    [Fact]
    public void History_Origin_Returns_To_History_And_Hides_Edit_Actions()
    {
        var transactionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var accountId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        RegisterServices(transactionId, accountId);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/transactions/{transactionId}?origin=history-transactions&year=2024");

        var cut = RenderComponent<TransactionDetailPage>(parameters => parameters.Add(x => x.Id, transactionId));

        cut.WaitForAssertion(() =>
        {
            cut.Find("a.btn.btn-outline-secondary");
        });

        cut.FindAll("a.btn.btn-primary").Should().BeEmpty();
        cut.Find("a.btn.btn-outline-secondary").GetAttribute("href").Should().Be("/history/transactions?year=2024");
    }

    [Fact]
    public void History_Movements_Origin_Returns_To_History_Movements_With_Context()
    {
        var transactionId = Guid.Parse("abababab-abab-abab-abab-abababababab");
        var accountId = Guid.Parse("cdcdcdcd-cdcd-cdcd-cdcd-cdcdcdcdcdcd");
        RegisterServices(transactionId, accountId);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/transactions/{transactionId}?origin=history-movements&year=2025&accountId={accountId}");

        var cut = RenderComponent<TransactionDetailPage>(parameters => parameters.Add(x => x.Id, transactionId));

        cut.WaitForAssertion(() =>
        {
            cut.Find("a.btn.btn-outline-secondary");
        });

        cut.FindAll("a.btn.btn-primary").Should().BeEmpty();

        var backHref = cut.Find("a.btn.btn-outline-secondary").GetAttribute("href");
        backHref.Should().Contain("/history/movements");
        backHref.Should().Contain("year=2025");
        backHref.Should().Contain($"accountId={accountId}");
    }

    [Fact]
    public void Report_Origin_Returns_To_Account_Movements_With_Report_Context()
    {
        var transactionId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var accountId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        RegisterServices(transactionId, accountId);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/transactions/{transactionId}?origin=report-category-totals&accountId={accountId}&from=2026-01-01&to=2026-02-01");

        var cut = RenderComponent<TransactionDetailPage>(parameters => parameters.Add(x => x.Id, transactionId));

        cut.WaitForAssertion(() =>
        {
            cut.Find("a.btn.btn-outline-secondary");
        });

        var backHref = cut.Find("a.btn.btn-outline-secondary").GetAttribute("href");
        backHref.Should().Contain($"/accounts/{accountId}/movements");
        backHref.Should().Contain("origin=report-category-totals");
        backHref.Should().Contain($"accountId={accountId}");
        backHref.Should().Contain("from=2026-01-01");
        backHref.Should().Contain("to=2026-02-01");
    }

    [Fact]
    public void Unknown_Origin_Falls_Back_To_Transactions()
    {
        var transactionId = Guid.Parse("12121212-1212-1212-1212-121212121212");
        var accountId = Guid.Parse("34343434-3434-3434-3434-343434343434");
        RegisterServices(transactionId, accountId);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo($"/transactions/{transactionId}?origin=https://evil.example.com");

        var cut = RenderComponent<TransactionDetailPage>(parameters => parameters.Add(x => x.Id, transactionId));

        cut.WaitForAssertion(() =>
        {
            cut.Find("a.btn.btn-outline-secondary");
        });

        cut.Find("a.btn.btn-outline-secondary").GetAttribute("href").Should().Be("/transactions");
    }

    private void RegisterServices(Guid transactionId, Guid accountId)
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

                if (uri.Contains($"api/v1/transactions/{transactionId}", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new TransactionDto(
                            transactionId,
                            new DateOnly(2026, 2, 14),
                            "Weekly groceries",
                            null,
                            "Supermarket",
                            [
                                new TransactionSplitDto(accountId, -50m, null),
                                new TransactionSplitDto(Guid.Parse("56565656-5656-5656-5656-565656565656"), 50m, null)
                            ]))
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

        var accountsApiMock = new Mock<IAccountsApi>(MockBehavior.Strict);
        accountsApiMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AccountDto(
                    accountId,
                    "Main Bank",
                    AccountNature.Asset,
                    AccountKind.Checking,
                    new DateOnly(2026, 1, 1),
                    false,
                    null),
                new AccountDto(
                    Guid.Parse("56565656-5656-5656-5656-565656565656"),
                    "Groceries",
                    AccountNature.Expense,
                    AccountKind.ExpenseCategory,
                    new DateOnly(2026, 1, 1),
                    false,
                    null)
            ]);

        var tokenStore = new TestTokenStore("test-token");
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddSingleton(accountsApiMock.Object);
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
