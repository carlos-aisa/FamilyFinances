using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class MonthlySummaryTests
{
    [Fact]
    public async Task MonthlySummary_Returns_Correct_Totals_For_Given_Month()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Arrange
        var bank = await LedgerApiTests.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var salary = await LedgerApiTests.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await LedgerApiTests.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // January 2026 income
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -100_000, memo = "Salary in" },
                new { accountId = salary.Id, amountCents = 100_000, memo = "Salary" }
            }
        });

        // January 2026 expense
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 20_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -20_000, memo = "Expense" }
            }
        });

        // February (must be ignored)
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-02-01",
            description = "Later expense",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 5_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -5_000, memo = "Expense" }
            }
        });

        // Act
        var res = await client.GetAsync("/api/v1/reports/monthly-summary?year=2026&month=1");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await res.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        summary.Should().NotBeNull();

        summary!.Year.Should().Be(2026);
        summary.Month.Should().Be(1);
        summary.IncomeTotal.Should().Be(100_000);
        summary.ExpenseTotal.Should().Be(20_000);
        summary.Net.Should().Be(80_000);
        summary.TransactionsCount.Should().Be(2);
    }

    public sealed record MonthlySummaryDto(
        int Year,
        int Month,
        long IncomeTotal,
        long ExpenseTotal,
        long Net,
        int TransactionsCount);
}
