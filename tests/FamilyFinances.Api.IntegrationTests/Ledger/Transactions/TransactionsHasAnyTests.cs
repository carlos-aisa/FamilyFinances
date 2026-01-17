using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Transactions;

public sealed class TransactionsHasAnyTests
{
    [Fact]
    public async Task HasAny_ReturnsFalse_WhenNoTransactionsExist()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync("/api/v1/transactions/any");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HasAnyResponse>();
        result.Should().NotBeNull();
        result!.HasAny.Should().BeFalse();
    }

    [Fact]
    public async Task HasAny_ReturnsTrue_AfterCreatingTransaction()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create accounts
        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create a transaction
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5000, memo = "Expense" }
            }
        });

        // Check HasAny
        var response = await client.GetAsync("/api/v1/transactions/any");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<HasAnyResponse>();
        result.Should().NotBeNull();
        result!.HasAny.Should().BeTrue();
    }

    [Fact]
    public async Task HasAny_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/transactions/any");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record HasAnyResponse(bool HasAny);
}
