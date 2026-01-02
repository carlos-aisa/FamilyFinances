using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests;

public sealed class TransactionsPayeesApiTests
{
    [Fact]
    public async Task Can_CreateTransaction_WithPayee()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create payee
        var payeeRes = await client.PostAsJsonAsync("/api/v1/payees", new { name = "Mercadona" });
        payeeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var payee = await payeeRes.Content.ReadFromJsonAsync<PayeeDto>();
        payee!.Id.Should().NotBeEmpty();

        // Create accounts (adjust this to your helpers / endpoints if needed)
        var a1 = await LedgerApiTests.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var a2 = await LedgerApiTests.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create transaction with payee
        var txRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Groceries",
            payeeId = payee.Id,
            splits = new[]
            {
                new { accountId = a1.Id, amountCents = -5000L, memo = (string?)null },
                new { accountId = a2.Id, amountCents =  5000L, memo = (string?)null },
            }
        });

        txRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var tx = await txRes.Content.ReadFromJsonAsync<TransactionDto>();
        tx!.PayeeId.Should().Be(payee.Id);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenPayeeDoesNotExist()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var a1 = await LedgerApiTests.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var a2 = await LedgerApiTests.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        var missingPayeeId = Guid.NewGuid();

        var txRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Groceries",
            payeeId = missingPayeeId,
            splits = new[]
            {
                new { accountId = a1.Id, amountCents = -5000L, memo = (string?)null },
                new { accountId = a2.Id, amountCents =  5000L, memo = (string?)null },
            }
        });

        txRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record PayeeDto(Guid Id, string Name);
    private sealed record TransactionDto(Guid Id, string Description, Guid? PayeeId);
}
