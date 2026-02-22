using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class MonthlySummaryTests
{
    [Fact]
    public async Task MonthlySummary_Returns_Correct_Totals_For_Given_Month()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Arrange
        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var income = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // January 2026 income
        // Correct accounting: Asset increases (debit, positive), Income decreases (credit, negative)
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 100_000, memo = "Salary in" },  // Asset increase
                new { accountId = income.Id, amountCents = -100_000, memo = "Salary" }    // Income credit
            }
        });

        // January 2026 expense
        // Correct accounting: Asset decreases (credit, negative), Expense increases (debit, positive)
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -20_000, memo = "Payment" },     // Asset decrease
                new { accountId = groceries.Id, amountCents = 20_000, memo = "Expense" }  // Expense debit
            }
        });

        // February (must be ignored)
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-02-01",
            description = "Later expense",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5_000, memo = "Expense" }
            }
        });

        // Act
        var res = await client.GetAsync("/api/v1/reports/monthly-summary?from=2026-01-01&to=2026-02-01");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await res.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        summary.Should().NotBeNull();

        summary!.From.Should().Be(new DateOnly(2026, 1, 1));
        summary.To.Should().Be(new DateOnly(2026, 2, 1));
        // New sign convention: Income is positive, Expenses are negative
        summary.IncomeTotal.Should().Be(100_000);  // Income stored as -100k, displayed as +100k
        summary.ExpenseTotal.Should().Be(-20_000); // Expense stored as +20k, displayed as -20k
        summary.Net.Should().Be(80_000);           // 100k + (-20k) = 80k
        summary.TransactionsCount.Should().Be(2);
    }

    [Fact]
    public async Task MonthlySummary_With_Income_Account_Filter_Maps_Total_To_Income_Kpi()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var income = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 100_000, memo = "Salary in" },
                new { accountId = income.Id, amountCents = -100_000, memo = "Income credit" }
            }
        });

        var res = await client.GetAsync(
            $"/api/v1/reports/monthly-summary?from=2026-01-01&to=2026-02-01&accountId={income.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await res.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        summary.Should().NotBeNull();
        summary!.IncomeTotal.Should().Be(100_000);
        summary.ExpenseTotal.Should().Be(0);
        summary.Net.Should().Be(100_000);
        summary.TransactionsCount.Should().Be(1);
    }

    [Fact]
    public async Task MonthlySummary_With_Payee_Filter_Returns_Filtered_Totals()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        var payeeRes = await client.PostAsJsonAsync("/api/v1/payees", new { name = "Mercadona" });
        payeeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var payee = await payeeRes.Content.ReadFromJsonAsync<PayeeDto>();
        payee.Should().NotBeNull();

        // Transaction with target payee
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries with payee",
            payeeId = payee!.Id,
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -20_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 20_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        // Transaction without payee (must be excluded)
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-11",
            description = "Groceries without payee",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        var res = await client.GetAsync(
            $"/api/v1/reports/monthly-summary?from=2026-01-01&to=2026-02-01&payeeId={payee!.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await res.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        summary.Should().NotBeNull();
        summary!.ExpenseTotal.Should().Be(-20_000);
        summary.TransactionsCount.Should().Be(1);
    }

    public sealed record MonthlySummaryDto(
        DateOnly From,
        DateOnly To,
        long IncomeTotal,
        long ExpenseTotal,
        long Net,
        int TransactionsCount);

    private sealed record PayeeDto(Guid Id, string Name);
}
