using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Transactions;

public sealed class TransactionsApiTests
{
    [Fact]
    public async Task Creating_Unbalanced_Transaction_Returns_400()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        var txRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Bad Tx",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 2000, memo = "Expense" }
            }
        });

        txRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await txRes.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("balanced");
    }

    [Fact]
    public async Task Can_Create_Balanced_Transaction_And_Get_ById()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create balanced transaction
        var createTxRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5000, memo = "Expense" }
            }
        });

        createTxRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdTx = await createTxRes.Content.ReadFromJsonAsync<TransactionDto>();
        createdTx.Should().NotBeNull();
        createdTx!.Id.Should().NotBeEmpty();
        createdTx.Splits.Should().HaveCount(2);
        createdTx.Splits.Sum(s => s.AmountCents).Should().Be(0);

        // Get by id
        var getRes = await client.GetAsync($"/api/v1/transactions/{createdTx.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getRes.Content.ReadFromJsonAsync<TransactionDto>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(createdTx.Id);
        fetched.Description.Should().Be("Groceries");
        fetched.Splits.Should().HaveCount(2);
        fetched.Splits.Sum(s => s.AmountCents).Should().Be(0);

        fetched.Splits.Should().Contain(s => s.AccountId == bank.Id && s.AmountCents == -5000);
        fetched.Splits.Should().Contain(s => s.AccountId == groceries.Id && s.AmountCents == 5000);
    }

    [Fact]
    public async Task Create_Transaction_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-02",
            description = "Test",
            splits = Array.Empty<object>()
        });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public sealed record TransactionDto(
        Guid Id,
        string Description,
        List<TransactionSplitDto> Splits);

    public sealed record TransactionSplitDto(
        Guid AccountId,
        long AmountCents,
        string? Memo);

    public sealed record ErrorResponse(string Error);
}
