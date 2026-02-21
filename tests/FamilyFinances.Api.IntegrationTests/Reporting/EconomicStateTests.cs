using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class EconomicStateTests
{
    [Fact]
    public async Task EconomicState_Returns_Stock_And_Flow_Kpis_For_Selected_AsOf_Date()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var mortgage = await TestHelpers.CreateAccountAsync(client, "Mortgage Principal", "Liability", "Loan");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var openingBalance = await TestHelpers.CreateAccountAsync(client, "Initial Equity", "Equity", "Other");

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-01",
            description = "Opening balances",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 300_000, memo = "Asset opening" },
                new { accountId = mortgage.Id, amountCents = -200_000, memo = "Liability opening" },
                new { accountId = openingBalance.Id, amountCents = -100_000, memo = "Balancing equity" }
            }
        })).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "January salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 100_000, memo = "Income in bank" },
                new { accountId = salary.Id, amountCents = -100_000, memo = "Income source" }
            }
        })).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -30_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 30_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Mortgage principal payment",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -50_000, memo = "Asset decrease" },
                new { accountId = mortgage.Id, amountCents = 50_000, memo = "Liability reduction" }
            }
        })).EnsureSuccessStatusCode();

        // Excluded from as-of January 31st
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-02-01",
            description = "February salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 70_000, memo = "Income in bank" },
                new { accountId = salary.Id, amountCents = -70_000, memo = "Income source" }
            }
        })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/v1/reports/economic-state?asOf=2026-01-31");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<EconomicStateDto>();
        dto.Should().NotBeNull();

        dto!.AsOf.Should().Be(new DateOnly(2026, 1, 31));
        dto.AssetsTotalCents.Should().Be(320_000);
        dto.LiabilitiesTotalCents.Should().Be(150_000);
        dto.NetWorthCents.Should().Be(170_000);
        dto.IncomeTotalCents.Should().Be(100_000);
        dto.ExpenseTotalCents.Should().Be(-30_000);
        dto.PeriodNetResultCents.Should().Be(70_000);
    }

    [Fact]
    public async Task EconomicState_Without_AsOf_Returns_BadRequest()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync("/api/v1/reports/economic-state");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EconomicState_With_Invalid_AsOf_Returns_BadRequest()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync("/api/v1/reports/economic-state?asOf=invalid-date");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record EconomicStateDto(
        DateOnly AsOf,
        long AssetsTotalCents,
        long LiabilitiesTotalCents,
        long NetWorthCents,
        long IncomeTotalCents,
        long ExpenseTotalCents,
        long PeriodNetResultCents
    );
}
