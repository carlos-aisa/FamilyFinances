using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class AccountTotalsTests
{
    [Fact]
    public async Task AccountTotals_Returns_NetChange_Per_Account_For_Period()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await LedgerApiTests.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await LedgerApiTests.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await LedgerApiTests.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Jan income
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -100_000, memo = "Salary in" },
                new { accountId = salary.Id, amountCents = 100_000, memo = "Salary" }
            }
        })).EnsureSuccessStatusCode();

        // Jan expense
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 20_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -20_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        // Feb expense (ignored)
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-02-01",
            description = "Later expense",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 5_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -5_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        // Act
        var res = await client.GetAsync(
            "/api/v1/reports/account-totals?from=2026-01-01&to=2026-02-01&includeZeroAccounts=false");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountTotalsDto>();
        dto.Should().NotBeNull();
        dto!.Items.Should().NotBeNull();

        var bankItem = dto.Items.Single(i => i.AccountId == bank.Id);
        var salaryItem = dto.Items.Single(i => i.AccountId == salary.Id);
        var groceriesItem = dto.Items.Single(i => i.AccountId == groceries.Id);

        bankItem.NetChange.Should().Be(-80_000);      // -100000 + 20000
        salaryItem.NetChange.Should().Be(100_000);
        groceriesItem.NetChange.Should().Be(-20_000);

        bankItem.TransactionsCount.Should().Be(2);
        salaryItem.TransactionsCount.Should().Be(1);
        groceriesItem.TransactionsCount.Should().Be(1);
    }

    public sealed record AccountTotalsDto(
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        List<AccountTotalItemDto> Items);

    public sealed record AccountTotalItemDto(
        Guid AccountId,
        string AccountName,
        AccountNature AccountNature,
        AccountKind AccountKind,
        long NetChange,
        int TransactionsCount);
}
