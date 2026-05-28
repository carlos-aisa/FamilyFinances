using System.Net;
using System.Net.Http.Json;
using System.Globalization;
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
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        try
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
        cut.Markup.Should().Contain("€");
        cut.Markup.Should().NotContain("$");
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
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

    [Fact]
    public void Transactions_Search_IsAccentInsensitive_ForHeadlineSubheadlineAndPayee()
    {
        RegisterServices(
        [
            new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 10),
                "José transfer",
                "Standard movement",
                "Alpha",
                -40m,
                TransactionListItemType.Expense),
            new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 11),
                "Utilities",
                "Comisión de luz",
                "Beta",
                -65m,
                TransactionListItemType.Expense),
            new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 12),
                "Groceries",
                "Weekly food",
                "María Market",
                -30m,
                TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        var searchInput = cut.Find("input[placeholder='Description or payee...']");
        var applyButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase));
        var resetButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Reset", StringComparison.OrdinalIgnoreCase));

        searchInput.Input("jose");
        applyButton.Click();
        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(1);
            rows[0].TextContent.Should().Contain("José transfer");
        });

        resetButton.Click();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        searchInput = cut.Find("input[placeholder='Description or payee...']");
        searchInput.Input("comision");
        applyButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase));
        applyButton.Click();
        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(1);
            rows[0].TextContent.Should().Contain("Comisión de luz");
        });

        resetButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Reset", StringComparison.OrdinalIgnoreCase));
        resetButton.Click();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        searchInput = cut.Find("input[placeholder='Description or payee...']");
        searchInput.Input("maria");
        applyButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase));
        applyButton.Click();
        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(1);
            rows[0].TextContent.Should().Contain("María Market");
        });
    }

    [Fact]
    public void Transactions_AmountRange_Filters_WithInclusiveBounds()
    {
        RegisterServices(
        [
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 10), "A", "A", "Payee A", 9.99m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 11), "B", "B", "Payee B", 10.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 12), "C", "C", "Payee C", 25.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 13), "D", "D", "Payee D", 50.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 14), "E", "E", "Payee E", 50.01m, TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(5));

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs.Should().HaveCount(2);
        numericInputs[0].Change("10");
        cut.FindAll("input[type='number']")[1].Change("50");

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(3);
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee B", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee C", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee D", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().NotContain(text => text.Contains("Payee A", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().NotContain(text => text.Contains("Payee E", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void Transactions_AmountRange_SupportsMinOnly_AndMaxOnly()
    {
        RegisterServices(
        [
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 10), "Low", "Low", "Payee A", 5.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 11), "Mid", "Mid", "Payee B", 25.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 12), "High", "High", "Payee C", 60.00m, TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs[0].Change("20");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(2);
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee B", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee C", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().NotContain(text => text.Contains("Payee A", StringComparison.Ordinal));
        });

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Reset", StringComparison.OrdinalIgnoreCase))
            .Click();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        numericInputs = cut.FindAll("input[type='number']");
        numericInputs[1].Change("30");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(2);
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee A", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee B", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().NotContain(text => text.Contains("Payee C", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void Transactions_AmountRange_InvalidRange_ShowsError_AndKeepsCurrentResults()
    {
        RegisterServices(
        [
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 10), "First", "First", "Payee A", 10.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 11), "Second", "Second", "Payee B", 20.00m, TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(2));

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs[0].Change("100");
        cut.FindAll("input[type='number']")[1].Change("50");

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Amount From must be less than or equal to Amount To.");
            cut.FindAll("tbody tr").Should().HaveCount(2);
        });
    }

    [Fact]
    public void Transactions_AmountRange_ValidRange_ClearsPreviousValidationError()
    {
        RegisterServices(
        [
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 10), "First", "First", "Payee A", 10.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 11), "Second", "Second", "Payee B", 20.00m, TransactionListItemType.Expense),
            new TransactionListItemDto(Guid.NewGuid(), new DateOnly(2026, 2, 12), "Third", "Third", "Payee C", 60.00m, TransactionListItemType.Expense)
        ]);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs[0].Change("100");
        cut.FindAll("input[type='number']")[1].Change("50");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Amount From must be less than or equal to Amount To.");
            cut.FindAll("tbody tr").Should().HaveCount(3);
        });

        numericInputs = cut.FindAll("input[type='number']");
        numericInputs[0].Change("10");
        cut.FindAll("input[type='number']")[1].Change("50");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain("Amount From must be less than or equal to Amount To.");
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(2);
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee A", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().Contain(text => text.Contains("Payee B", StringComparison.Ordinal));
            rows.Select(r => r.TextContent).Should().NotContain(text => text.Contains("Payee C", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void Transactions_AmountRange_LoadMore_AppendsFilteredRowsOnly()
    {
        var items = Enumerable.Range(1, 120)
            .Select(i => new TransactionListItemDto(
                Guid.NewGuid(),
                new DateOnly(2026, 2, 1).AddDays(i - 1),
                $"Headline {i:D3}",
                $"Subheadline {i:D3}",
                $"Payee {i:D3}",
                i,
                TransactionListItemType.Expense))
            .ToList();

        RegisterServices(items);

        var cut = RenderComponent<TransactionsListPage>();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(50));

        var numericInputs = cut.FindAll("input[type='number']");
        numericInputs[0].Change("20");
        cut.FindAll("input[type='number']")[1].Change("100");
        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Apply Filters", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(50);
            cut.Markup.Should().Contain("Payee 020");
            cut.Markup.Should().Contain("Payee 069");
            cut.Markup.Should().NotContain("Payee 019");
            cut.Markup.Should().NotContain("Payee 070");
        });

        cut.FindAll("button")
            .First(button => button.TextContent.Contains("Load More", StringComparison.OrdinalIgnoreCase))
            .Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("tbody tr");
            rows.Should().HaveCount(81);
            cut.Markup.Should().Contain("Payee 100");
            cut.Markup.Should().NotContain("Payee 019");
            cut.Markup.Should().NotContain("Payee 101");
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
