using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Accounts;

public sealed class AccountMovementsApiTests
{
    [Fact]
    public async Task GetMovements_Returns_CorrectSignedAmounts_ForAssetAccount()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        // Income: money INTO bank account (+100.00)
        await CreateTransactionAsync(client, "2026-01-05", "Salary payment", new[]
        {
            new { accountId = salary.Id, amountCents = 100_00, memo = "Salary" },
            new { accountId = bank.Id, amountCents = -100_00, memo = "Into bank" }
        });

        // Expense: money OUT OF bank account (-20.00)
        await CreateTransactionAsync(client, "2026-01-10", "Grocery shopping", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -20_00, memo = "Groceries" }
        });

        // Act
        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.AccountId.Should().Be(bank.Id);
        result.AccountName.Should().Be("Bank Account");
        result.Items.Should().HaveCount(2);

        // For bank account:
        // - Salary: SignedAmount = -100.00 (money INTO bank, negative split becomes negative amount) 
        // - Groceries: SignedAmount = +20.00 (money OUT OF bank, positive split becomes positive amount)
        var salaryMovement = result.Items.Single(m => m.Description == "Salary payment");
        var groceriesMovement = result.Items.Single(m => m.Description == "Grocery shopping");

        salaryMovement.SignedAmount.Should().Be(-100.00m);
        groceriesMovement.SignedAmount.Should().Be(20.00m);

        salaryMovement.CounterpartyAccountName.Should().Be("Salary");
        groceriesMovement.CounterpartyAccountName.Should().Be("Groceries");
    }

    [Fact]
    public async Task GetMovements_Returns_CorrectSignedAmounts_ForExpenseAccount()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Expense transaction: money INTO groceries account (-20.00 from groceries perspective)
        await CreateTransactionAsync(client, "2026-01-10", "Grocery shopping", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -20_00, memo = "Groceries" }
        });

        // Refund: money OUT OF groceries account (+5.00 from groceries perspective)
        await CreateTransactionAsync(client, "2026-01-15", "Grocery refund", new[]
        {
            new { accountId = groceries.Id, amountCents = 5_00, memo = "Refund" },
            new { accountId = bank.Id, amountCents = -5_00, memo = "Refunded" }
        });

        // Act - Get movements for groceries account
        var response = await client.GetAsync(
            $"/api/v1/accounts/{groceries.Id}/movements?from=2026-01-01&to=2026-02-01");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.AccountId.Should().Be(groceries.Id);
        result.Items.Should().HaveCount(2);

        // For groceries account:
        // - Expense: SignedAmount = -20.00 (expense increases this account, so negative split)
        // - Refund: SignedAmount = +5.00 (refund decreases this account, so positive split)
        var expenseMovement = result.Items.Single(m => m.Description == "Grocery shopping");
        var refundMovement = result.Items.Single(m => m.Description == "Grocery refund");

        expenseMovement.SignedAmount.Should().Be(-20.00m);
        refundMovement.SignedAmount.Should().Be(5.00m);
    }

    [Fact]
    public async Task GetMovements_AppliesSearchFilter()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await CreateTransactionAsync(client, "2026-01-10", "Grocery shopping", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -20_00, memo = "Groceries" }
        });

        await CreateTransactionAsync(client, "2026-01-15", "Gas station", new[]
        {
            new { accountId = bank.Id, amountCents = 30_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -30_00, memo = "Fuel" }
        });

        // Act - Search for "grocery"
        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&q=grocery");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(m => m.Description == "Grocery shopping");
        result.Items.Should().NotContain(m => m.Description == "Gas station");
    }

    [Fact]
    public async Task GetMovements_AppliesMinAmountFilter_OnAbsoluteAmount()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        await CreateTransactionAsync(client, "2026-01-10", "Small movement", new[]
        {
            new { accountId = bank.Id, amountCents = 5_00, memo = "Small payment" },
            new { accountId = expense.Id, amountCents = -5_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-11", "Medium movement", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Medium payment" },
            new { accountId = expense.Id, amountCents = -20_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-12", "Large movement", new[]
        {
            new { accountId = bank.Id, amountCents = 80_00, memo = "Large payment" },
            new { accountId = expense.Id, amountCents = -80_00, memo = "Expense" }
        });

        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&minAmount=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Select(x => x.Description).Should().BeEquivalentTo(["Large movement", "Medium movement"]);
    }

    [Fact]
    public async Task GetMovements_AppliesMaxAmountFilter_OnAbsoluteAmount()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        await CreateTransactionAsync(client, "2026-01-10", "Small movement", new[]
        {
            new { accountId = bank.Id, amountCents = 5_00, memo = "Small payment" },
            new { accountId = expense.Id, amountCents = -5_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-11", "Medium movement", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Medium payment" },
            new { accountId = expense.Id, amountCents = -20_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-12", "Large movement", new[]
        {
            new { accountId = bank.Id, amountCents = 80_00, memo = "Large payment" },
            new { accountId = expense.Id, amountCents = -80_00, memo = "Expense" }
        });

        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&maxAmount=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Select(x => x.Description).Should().BeEquivalentTo(["Medium movement", "Small movement"]);
    }

    [Fact]
    public async Task GetMovements_AppliesInclusiveBoundedAmountFilter()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        await CreateTransactionAsync(client, "2026-01-10", "Edge low", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Low edge" },
            new { accountId = expense.Id, amountCents = -20_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-11", "Inside", new[]
        {
            new { accountId = bank.Id, amountCents = 35_00, memo = "Inside" },
            new { accountId = expense.Id, amountCents = -35_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-12", "Edge high", new[]
        {
            new { accountId = bank.Id, amountCents = 50_00, memo = "High edge" },
            new { accountId = expense.Id, amountCents = -50_00, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2026-01-13", "Outside high", new[]
        {
            new { accountId = bank.Id, amountCents = 51_00, memo = "Outside" },
            new { accountId = expense.Id, amountCents = -51_00, memo = "Expense" }
        });

        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&minAmount=20&maxAmount=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Select(x => x.Description).Should().BeEquivalentTo(["Edge high", "Inside", "Edge low"]);
    }

    [Fact]
    public async Task GetMovements_AmountRange_UsesAbsoluteValue_ForBothSignedDirections()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await CreateTransactionAsync(client, "2026-01-10", "Income thirty", new[]
        {
            new { accountId = salary.Id, amountCents = 30_00, memo = "Salary" },
            new { accountId = bank.Id, amountCents = -30_00, memo = "Into bank" }
        });

        await CreateTransactionAsync(client, "2026-01-11", "Expense thirty", new[]
        {
            new { accountId = bank.Id, amountCents = 30_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -30_00, memo = "Groceries" }
        });

        await CreateTransactionAsync(client, "2026-01-12", "Outside sixty", new[]
        {
            new { accountId = bank.Id, amountCents = 60_00, memo = "Outside" },
            new { accountId = groceries.Id, amountCents = -60_00, memo = "Groceries" }
        });

        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&minAmount=10&maxAmount=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Select(x => x.Description).Should().BeEquivalentTo(["Expense thirty", "Income thirty"]);
        result.Items.Should().NotContain(x => x.Description == "Outside sixty");
    }

    [Fact]
    public async Task GetMovements_SupportsPagination()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        // Create 5 transactions
        for (int i = 1; i <= 5; i++)
        {
            await CreateTransactionAsync(client, $"2026-01-{i:D2}", $"Transaction {i}", new[]
            {
                new { accountId = bank.Id, amountCents = i * 1000, memo = "Payment" },
                new { accountId = expense.Id, amountCents = -i * 1000, memo = "Expense" }
            });
        }

        // Act - Get first page with page size 2
        var response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-02-01&page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);

        // Should be ordered by date descending (newest first)
        result.Items[0].Description.Should().Be("Transaction 5");
        result.Items[1].Description.Should().Be("Transaction 4");
        result.Items[0].RunningBalance.Should().Be(150.00m);
        result.Items[1].RunningBalance.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetMovements_Pagination_MoreThanFiftyRows_KeepsRunningBalanceCorrectAcrossPages()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        var start = new DateOnly(2026, 1, 1);
        for (int i = 1; i <= 55; i++)
        {
            await CreateTransactionAsync(client, start.AddDays(i - 1).ToString("yyyy-MM-dd"), $"Movement {i:D2}", new[]
            {
                new { accountId = bank.Id, amountCents = 100, memo = $"Payment {i:D2}" },
                new { accountId = expense.Id, amountCents = -100, memo = $"Expense {i:D2}" }
            });
        }

        var firstPageResponse = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-04-01&page=1&pageSize=50");
        firstPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<AccountMovementsDto>();

        firstPage.Should().NotBeNull();
        firstPage!.Items.Should().HaveCount(50);
        firstPage.TotalCount.Should().Be(55);
        firstPage.Items[0].Description.Should().Be("Movement 55");
        firstPage.Items[0].RunningBalance.Should().Be(55.00m);
        firstPage.Items[^1].Description.Should().Be("Movement 06");
        firstPage.Items[^1].RunningBalance.Should().Be(6.00m);

        var secondPageResponse = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-04-01&page=2&pageSize=50");
        secondPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<AccountMovementsDto>();

        secondPage.Should().NotBeNull();
        secondPage!.Items.Should().HaveCount(5);
        secondPage.TotalCount.Should().Be(55);
        secondPage.Items[0].Description.Should().Be("Movement 05");
        secondPage.Items[0].RunningBalance.Should().Be(5.00m);
        secondPage.Items[^1].Description.Should().Be("Movement 01");
        secondPage.Items[^1].RunningBalance.Should().Be(1.00m);
    }

    [Fact]
    public async Task GetMovements_Pagination_WithFilters_KeepsRunningBalanceIndependentOfPageSize()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        var start = new DateOnly(2026, 1, 1);
        for (int i = 1; i <= 30; i++)
        {
            await CreateTransactionAsync(client, start.AddDays(i - 1).ToString("yyyy-MM-dd"), $"Filter movement {i:D2}", new[]
            {
                new { accountId = bank.Id, amountCents = 100, memo = $"Payment {i:D2}" },
                new { accountId = expense.Id, amountCents = -100, memo = $"Expense {i:D2}" }
            });
        }

        var page1Size20Response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-03-01&q=filter&page=1&pageSize=20");
        page1Size20Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page1Size20 = await page1Size20Response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        var page2Size10Response = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2026-01-01&to=2026-03-01&q=filter&page=2&pageSize=10");
        page2Size10Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page2Size10 = await page2Size10Response.Content.ReadFromJsonAsync<AccountMovementsDto>();

        page1Size20.Should().NotBeNull();
        page2Size10.Should().NotBeNull();
        page1Size20!.TotalCount.Should().Be(30);
        page2Size10!.TotalCount.Should().Be(30);

        var movement15From20 = page1Size20.Items.Single(x => x.Description == "Filter movement 15");
        var movement15From10 = page2Size10.Items.Single(x => x.Description == "Filter movement 15");

        movement15From20.RunningBalance.Should().Be(15.00m);
        movement15From10.RunningBalance.Should().Be(15.00m);
        movement15From10.RunningBalance.Should().Be(movement15From20.RunningBalance);
    }

    [Fact]
    public async Task GetMovements_Returns404_ForNonExistentAccount()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var nonExistentId = Guid.NewGuid();

        var response = await client.GetAsync(
            $"/api/v1/accounts/{nonExistentId}/movements?from=2026-01-01&to=2026-02-01");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMovements_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient(); // Not authorized

        var someId = Guid.NewGuid();

        var response = await client.GetAsync(
            $"/api/v1/accounts/{someId}/movements");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task CreateTransactionAsync(HttpClient client, string bookedOn, string description, object[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn,
            description,
            splits
        });

        response.EnsureSuccessStatusCode();
    }

    public sealed record AccountMovementsDto(
        Guid AccountId,
        string AccountName,
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        List<AccountMovementDto> Items,
        int TotalCount
    );

    public sealed record AccountMovementDto(
        Guid TransactionId,
        DateOnly BookedOn,
        string Description,
        string? PayeeName,
        decimal SignedAmount,
        string? CounterpartyAccountName,
        decimal RunningBalance
    );
}
