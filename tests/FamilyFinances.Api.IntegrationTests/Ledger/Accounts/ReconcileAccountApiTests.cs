using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Accounts;

public sealed class ReconcileAccountApiTests
{
    [Fact]
    public async Task Reconcile_CreatesPositiveAdjustment_WhenActualBalanceIsHigher()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create an expense: bank loses 50 euros
        await CreateTransactionAsync(client, "2026-01-10", "Shopping", new[]
        {
            new { accountId = bank.Id, amountCents = 5000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -5000, memo = "Expense" }
        });

        // Act: Reconcile bank account with actual balance of 30 euros
        // Current computed balance: +50 (money out)
        // Actual balance: +30
        // Difference: +30 - (+50) = -20 (need to remove 20)
        var reconcileRequest = new
        {
            actualBalance = 30m,
            asOfDate = "2026-01-15",
            note = "Found missing deposit"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{bank.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ReconcileResponse>();
        result.Should().NotBeNull();
        result!.AdjustmentCreated.Should().BeTrue();
        result.TransactionId.Should().NotBeNull();
        result.ComputedBalance.Should().Be(50m);
        result.ActualBalance.Should().Be(30m);
        result.Difference.Should().Be(-20m);

        // Verify balance after reconciliation
        var balancesResponse = await client.GetAsync("/api/v1/accounts/balances");
        var balances = await balancesResponse.Content.ReadFromJsonAsync<List<AccountBalanceDto>>();
        var bankBalance = balances!.Single(b => b.AccountId == bank.Id);
        bankBalance.Balance.Should().Be(30m, "because reconciliation should adjust the balance");
    }

    [Fact]
    public async Task Reconcile_CreatesNegativeAdjustment_WhenActualBalanceIsLower()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var cash = await TestHelpers.CreateAccountAsync(client, "Cash", "Asset", "Cash");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // No transactions, so computed balance is 0
        // Act: Reconcile cash account with actual balance of 10 euros
        // Difference: 10 - 0 = 10 (need to add 10)
        var reconcileRequest = new
        {
            actualBalance = 10m,
            asOfDate = "2026-01-15",
            note = "Missing income"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{cash.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ReconcileResponse>();
        result.Should().NotBeNull();
        result!.AdjustmentCreated.Should().BeTrue();
        result.TransactionId.Should().NotBeNull();
        result.ComputedBalance.Should().Be(0m);
        result.ActualBalance.Should().Be(10m);
        result.Difference.Should().Be(10m);

        // Verify balance after reconciliation
        var balancesResponse = await client.GetAsync("/api/v1/accounts/balances");
        var balances = await balancesResponse.Content.ReadFromJsonAsync<List<AccountBalanceDto>>();
        var cashBalance = balances!.Single(b => b.AccountId == cash.Id);
        cashBalance.Balance.Should().Be(10m, "because reconciliation should adjust the balance");
    }

    [Fact]
    public async Task Reconcile_DoesNotCreateTransaction_WhenBalanceMatches()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        // Create transaction: bank loses 50 euros
        await CreateTransactionAsync(client, "2026-01-10", "Expense", new[]
        {
            new { accountId = bank.Id, amountCents = 5000, memo = "Payment" },
            new { accountId = expense.Id, amountCents = -5000, memo = "Expense" }
        });

        // Act: Reconcile with exact balance
        var reconcileRequest = new
        {
            actualBalance = 50m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{bank.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ReconcileResponse>();
        result.Should().NotBeNull();
        result!.AdjustmentCreated.Should().BeFalse();
        result.TransactionId.Should().BeNull();
        result.ComputedBalance.Should().Be(50m);
        result.ActualBalance.Should().Be(50m);
        result.Difference.Should().Be(0m);
        result.Message.Should().Contain("already reconciled");
    }

    [Fact]
    public async Task Reconcile_ComputesBalanceAsOfDate()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        // Transaction on Jan 10: -50
        await CreateTransactionAsync(client, "2026-01-10", "Expense 1", new[]
        {
            new { accountId = bank.Id, amountCents = 5000, memo = "Payment" },
            new { accountId = expense.Id, amountCents = -5000, memo = "Expense" }
        });

        // Transaction on Jan 20: -30 (should NOT be included in reconciliation as-of Jan 15)
        await CreateTransactionAsync(client, "2026-01-20", "Expense 2", new[]
        {
            new { accountId = bank.Id, amountCents = 3000, memo = "Payment" },
            new { accountId = expense.Id, amountCents = -3000, memo = "Expense" }
        });

        // Act: Reconcile as of Jan 15 (should only consider first transaction)
        var reconcileRequest = new
        {
            actualBalance = 50m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{bank.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ReconcileResponse>();
        result.Should().NotBeNull();
        result!.ComputedBalance.Should().Be(50m, "only first transaction should be included");
        result.AdjustmentCreated.Should().BeFalse();
    }

    [Fact]
    public async Task Reconcile_Returns404_ForNonExistentAccount()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var nonExistentId = Guid.NewGuid();

        // Act
        var reconcileRequest = new
        {
            actualBalance = 100m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{nonExistentId}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reconcile_ReturnsError_ForNonAssetOrLiabilityAccount()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var expense = await TestHelpers.CreateAccountAsync(client, "Expenses", "Expense", "Other");

        // Act: Try to reconcile an expense account
        var reconcileRequest = new
        {
            actualBalance = 100m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{expense.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("Asset");
        error.Error.Should().Contain("Liability");
    }

    [Fact]
    public async Task Reconcile_RequiresAuth()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient(); // Not authorized

        var someId = Guid.NewGuid();

        // Act
        var reconcileRequest = new
        {
            actualBalance = 100m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{someId}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reconcile_CreatesAdjustmentAccounts_WhenMissing()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");

        // Act: Reconcile (should auto-create adjustment accounts)
        var reconcileRequest = new
        {
            actualBalance = 100m,
            asOfDate = "2026-01-15",
            note = (string?)null
        };

        var response = await client.PostAsJsonAsync(
            $"/api/v1/accounts/{bank.Id}/reconcile",
            reconcileRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify adjustment account was created
        var accountsResponse = await client.GetAsync("/api/v1/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountDto>>();
        accounts.Should().Contain(a =>
            (a.Name == "Balance Adjustments (Expense)" || a.Name == "Balance Adjustments") &&
            a.Nature == (int)AccountNature.Expense);
    }

    private static async Task CreateTransactionAsync(
        HttpClient client,
        string bookedOn,
        string description,
        object[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn,
            description,
            splits
        });

        response.EnsureSuccessStatusCode();
    }

    public sealed record ReconcileResponse(
        bool AdjustmentCreated,
        Guid? TransactionId,
        decimal ComputedBalance,
        decimal ActualBalance,
        decimal Difference,
        string Message
    );

    public sealed record AccountBalanceDto(Guid AccountId, decimal Balance);

    public sealed record AccountDto(
        Guid Id,
        string Name,
        int Nature,
        int Kind,
        DateOnly OpenedOn,
        bool IsClosed
    );

    public sealed record ErrorResponse(string Error);
}
