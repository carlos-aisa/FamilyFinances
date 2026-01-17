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
        createdTx.Splits.Sum(s => s.Amount).Should().Be(0);

        // Get by id
        var getRes = await client.GetAsync($"/api/v1/transactions/{createdTx.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getRes.Content.ReadFromJsonAsync<TransactionDto>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(createdTx.Id);
        fetched.Description.Should().Be("Groceries");
        fetched.Splits.Should().HaveCount(2);
        fetched.Splits.Sum(s => s.Amount).Should().Be(0);

        fetched.Splits.Should().Contain(s => s.AccountId == bank.Id && s.Amount == -50);
        fetched.Splits.Should().Contain(s => s.AccountId == groceries.Id && s.Amount == 50);
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

    [Fact]
    public async Task Can_Delete_Transaction()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create transaction
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

        // Delete transaction
        var deleteRes = await client.DeleteAsync($"/api/v1/transactions/{createdTx!.Id}");
        deleteRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getRes = await client.GetAsync($"/api/v1/transactions/{createdTx.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistent_Transaction_Returns_404()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var nonExistentId = Guid.NewGuid();
        var deleteRes = await client.DeleteAsync($"/api/v1/transactions/{nonExistentId}");

        deleteRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Transaction_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var someId = Guid.NewGuid();
        var res = await client.DeleteAsync($"/api/v1/transactions/{someId}");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_List_Transactions()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

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

        // List transactions
        var listRes = await client.GetAsync("/api/v1/transactions?take=10");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listRes.Content.ReadFromJsonAsync<List<TransactionListDto>>();
        list.Should().NotBeNull();
        list!.Should().HaveCount(1);
        list[0].Headline.Should().Be("Groceries");
    }

    [Fact]
    public async Task List_Transactions_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.GetAsync("/api/v1/transactions?take=10");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotFound()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var nonExistentId = Guid.NewGuid();
        var res = await client.GetAsync($"/api/v1/transactions/{nonExistentId}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var someId = Guid.NewGuid();
        var res = await client.GetAsync($"/api/v1/transactions/{someId}");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_Transaction_Returns204_And_Updates_Data()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create accounts
        var checking = await TestHelpers.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var savings = await TestHelpers.CreateAccountAsync(client, "Savings", "Asset", "Savings");

        // Create initial transaction
        var createRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Initial Transfer",
            splits = new[]
            {
                new { accountId = checking.Id, amountCents = -10000, memo = "Out" },
                new { accountId = savings.Id, amountCents = 10000, memo = "In" }
            }
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createRes.Content.ReadFromJsonAsync<TransactionDto>();
        created.Should().NotBeNull();
        var txId = created!.Id;

        // Update the transaction
        var updateRes = await client.PutAsJsonAsync($"/api/v1/transactions/{txId}", new
        {
            id = txId,
            bookedOn = "2026-01-10",
            description = "Updated Transfer",
            payeeId = (Guid?)null,
            fromAccountId = checking.Id,
            toAccountId = savings.Id,
            amount = 75.50m
        });

        // Verify PUT returns 204
        updateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get updated transaction and verify
        var getRes = await client.GetAsync($"/api/v1/transactions/{txId}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getRes.Content.ReadFromJsonAsync<TransactionDto>();
        updated.Should().NotBeNull();

        // Verify same Id
        updated!.Id.Should().Be(txId);

        // Verify new date
        updated.BookedOn.Should().Be(new DateOnly(2026, 1, 10));

        // Verify new description
        updated.Description.Should().Be("Updated Transfer");

        // Verify splits: 2 splits
        updated.Splits.Should().HaveCount(2);

        // Verify one negative
        updated.Splits.Should().Contain(s => s.AccountId == checking.Id && s.Amount < 0);

        // Verify one positive
        updated.Splits.Should().Contain(s => s.AccountId == savings.Id && s.Amount > 0);

        // Verify suma = 0
        updated.Splits.Sum(s => s.Amount).Should().Be(0);

        // Verify specific amounts
        updated.Splits.First(s => s.AccountId == checking.Id).Amount.Should().Be(-75.50m);
        updated.Splits.First(s => s.AccountId == savings.Id).Amount.Should().Be(75.50m);
    }

    [Fact]
    public async Task Update_NonExistent_Transaction_Returns404()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var checking = await TestHelpers.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var savings = await TestHelpers.CreateAccountAsync(client, "Savings", "Asset", "Savings");

        var nonExistentId = Guid.NewGuid();

        var updateRes = await client.PutAsJsonAsync($"/api/v1/transactions/{nonExistentId}", new
        {
            id = nonExistentId,
            bookedOn = "2026-01-10",
            description = "Test",
            payeeId = (Guid?)null,
            fromAccountId = checking.Id,
            toAccountId = savings.Id,
            amount = 50.00m
        });

        updateRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Transaction_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var someId = Guid.NewGuid();
        var updateRes = await client.PutAsJsonAsync($"/api/v1/transactions/{someId}", new
        {
            id = someId,
            bookedOn = "2026-01-10",
            description = "Test",
            payeeId = (Guid?)null,
            fromAccountId = Guid.NewGuid(),
            toAccountId = Guid.NewGuid(),
            amount = 50.00m
        });

        updateRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public sealed record TransactionDto(
        Guid Id,
        DateOnly BookedOn,
        string Description,
        List<TransactionSplitDto> Splits);

    public sealed record TransactionListDto(
        Guid Id,
        DateOnly BookedOn,
        string Headline,
        string? Subheadline,
        decimal Amount,
        int Type);

    public sealed record TransactionSplitDto(
        Guid AccountId,
        decimal Amount,
        string? Memo);

    public sealed record ErrorResponse(string Error);
}
