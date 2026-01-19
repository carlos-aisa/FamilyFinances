using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Accounts;

public sealed class AccountBalancesApiTests
{
    [Fact]
    public async Task GetBalances_Returns_CorrectBalances_ForMultipleAccounts()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        // Income: +100.00 to bank, +100.00 to salary
        await CreateTransactionAsync(client, "2026-01-05", "Salary payment", new[]
        {
            new { accountId = salary.Id, amountCents = 100_00, memo = "Salary" },
            new { accountId = bank.Id, amountCents = -100_00, memo = "Into bank" }
        });

        // Expense: +20.00 from bank, -20.00 to groceries
        await CreateTransactionAsync(client, "2026-01-10", "Grocery shopping", new[]
        {
            new { accountId = bank.Id, amountCents = 20_00, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -20_00, memo = "Groceries" }
        });

        // Act
        var response = await client.GetAsync("/api/v1/accounts/balances");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var balances = await response.Content.ReadFromJsonAsync<List<AccountBalanceDto>>();

        balances.Should().NotBeNull();
        balances.Should().HaveCount(3);

        var bankBalance = balances!.Single(b => b.AccountId == bank.Id);
        var groceriesBalance = balances.Single(b => b.AccountId == groceries.Id);
        var salaryBalance = balances.Single(b => b.AccountId == salary.Id);

        // Bank: -100.00 (inflow) + 20.00 (outflow) = -80.00
        bankBalance.Balance.Should().Be(-80.00m);

        // Groceries: -20.00 (expense)
        groceriesBalance.Balance.Should().Be(-20.00m);

        // Salary: +100.00 (income)
        salaryBalance.Balance.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetBalances_Returns_EmptyList_WhenNoTransactions()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create accounts but no transactions
        await TestHelpers.CreateAccountAsync(client, "Bank Account", "Asset", "Checking");
        await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Act
        var response = await client.GetAsync("/api/v1/accounts/balances");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var balances = await response.Content.ReadFromJsonAsync<List<AccountBalanceDto>>();

        balances.Should().NotBeNull();
        balances.Should().BeEmpty(); // No transactions = no balances returned
    }

    [Fact]
    public async Task GetBalances_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient(); // Not authorized

        var response = await client.GetAsync("/api/v1/accounts/balances");

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

    public sealed record AccountBalanceDto(Guid AccountId, decimal Balance);
}