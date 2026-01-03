using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class CategoryTotalsTests
{
    [Fact]
    public async Task CategoryTotals_Returns_Correct_Expense_Totals_For_Period()
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

        var res = await client.GetAsync(
            "/api/v1/reports/category-totals?from=2026-01-01&to=2026-02-01&nature=Expense");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<CategoryTotalsDto>();
        dto.Should().NotBeNull();
        dto!.Nature.Should().Be(AccountNature.Expense);

        dto.Items.Should().ContainSingle(i => i.AccountId == groceries.Id);
        var item = dto.Items.Single(i => i.AccountId == groceries.Id);

        item.Total.Should().Be(20_000);
        item.TransactionsCount.Should().Be(1);
    }

    public sealed record CategoryTotalsDto(
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        AccountNature Nature,
        List<CategoryTotalItemDto> Items);

    public sealed record CategoryTotalItemDto(
        Guid AccountId,
        string AccountName,
        long Total,
        int TransactionsCount);
}
