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
        string? CounterpartyAccountName
    );
}