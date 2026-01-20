using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class AccountGroupTotalsTests
{
    [Fact]
    public async Task AccountGroupTotals_Returns_Accumulated_Expense_For_Group_In_Period()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Accounts
        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var fixedBills = await TestHelpers.CreateAccountAsync(client, "Fixed Bills", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        // Create group
        var createGroupRes = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "House fixed expenses",
            description = "Recurring home expenses"
        });
        createGroupRes.EnsureSuccessStatusCode();

        var group = await createGroupRes.Content.ReadFromJsonAsync<AccountGroupDto>();
        group.Should().NotBeNull();

        // Add accounts to group (2 expense accounts)
        (await client.PostAsync($"/api/v1/account-groups/{group!.Id}/accounts/{groceries.Id}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostAsync($"/api/v1/account-groups/{group.Id}/accounts/{fixedBills.Id}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Transactions (January 2026)
        // Salary (ignored for expense report)
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

        // Groceries expense 20_000
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

        // Fixed bills expense 40_000
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Electricity",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 40_000, memo = "Payment" },
                new { accountId = fixedBills.Id, amountCents = -40_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        // February expense (ignored by date range)
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
            $"/api/v1/reports/account-groups/{group.Id}/totals?from=2026-01-01&to=2026-02-01");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupTotalsDto>();
        dto.Should().NotBeNull();

        dto!.GroupId.Should().Be(group.Id);
        dto.Nature.Should().Be(AccountNature.Expense);
        dto.TotalCents.Should().Be(60_000);
        dto.TransactionsCount.Should().Be(2);

        dto.Items.Should().HaveCount(2);
        dto.Items.Should().Contain(i => i.AccountId == groceries.Id && i.TotalCents == 20_000 && i.TransactionsCount == 1);
        dto.Items.Should().Contain(i => i.AccountId == fixedBills.Id && i.TotalCents == 40_000 && i.TransactionsCount == 1);
    }

    [Fact]
    public async Task AccountGroupTotals_Refunds_Subtract_From_Total()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Accounts
        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create group
        var createGroupRes = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Shopping",
            description = "Shopping expenses"
        });
        createGroupRes.EnsureSuccessStatusCode();

        var group = await createGroupRes.Content.ReadFromJsonAsync<AccountGroupDto>();
        group.Should().NotBeNull();

        // Add groceries account to group
        (await client.PostAsync($"/api/v1/account-groups/{group!.Id}/accounts/{groceries.Id}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Create expense: 50 euros
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Amazon purchase",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -5000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        // Create refund: 15 euros (reduces total expense)
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Amazon refund",
            splits = new[]
            {
                new { accountId = groceries.Id, amountCents = 1500, memo = "Refund" }, // Positive in expense account
                new { accountId = bank.Id, amountCents = -1500, memo = "Refund received" }
            }
        })).EnsureSuccessStatusCode();

        // Act
        var res = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/totals?from=2026-01-01&to=2026-02-01");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupTotalsDto>();
        dto.Should().NotBeNull();

        dto!.GroupId.Should().Be(group.Id);
        dto.Nature.Should().Be(AccountNature.Expense);
        
        // Net expense should be 50 - 15 = 35 euros = 3500 cents
        dto.TotalCents.Should().Be(3500, "because refund should subtract from total expenses");
        dto.TransactionsCount.Should().Be(2);

        dto.Items.Should().HaveCount(1);
        dto.Items[0].AccountId.Should().Be(groceries.Id);
        dto.Items[0].TotalCents.Should().Be(3500, "because 5000 (expense) - 1500 (refund) = 3500");
        dto.Items[0].TransactionsCount.Should().Be(2);
    }

    // minimal DTOs for test deserialization
    public sealed record AccountGroupDto(Guid Id, string Name, string? Description);

    public sealed record AccountGroupTotalsDto(
        Guid GroupId,
        string GroupName,
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        AccountNature Nature,
        long TotalCents,
        int TransactionsCount,
        int AccountsCount,
        List<AccountGroupTotalItemDto> Items);

    public sealed record AccountGroupTotalItemDto(
        Guid AccountId,
        string AccountName,
        long TotalCents,
        int TransactionsCount);
}
