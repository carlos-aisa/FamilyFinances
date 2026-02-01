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

    [Fact]
    public async Task Can_Create_And_Update_MultiSplit_Transaction()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create accounts for mortgage scenario
        var checking = await TestHelpers.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var mortgagePrincipal = await TestHelpers.CreateAccountAsync(client, "Mortgage Principal", "Liability", "Loan");
        var interestExpense = await TestHelpers.CreateAccountAsync(client, "Mortgage Interest", "Expense", "Other");

        // Create a 3-split transaction (mortgage payment)
        var createRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Mortgage Payment January",
            splits = new[]
            {
                new { accountId = checking.Id, amountCents = -120000, memo = "Payment from checking" },
                new { accountId = mortgagePrincipal.Id, amountCents = 80000, memo = "Principal reduction" },
                new { accountId = interestExpense.Id, amountCents = 40000, memo = "Interest expense" }
            }
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createRes.Content.ReadFromJsonAsync<TransactionDto>();
        created.Should().NotBeNull();
        created!.Splits.Should().HaveCount(3);
        created.Splits.Sum(s => s.Amount).Should().Be(0);

        var txId = created.Id;

        // Update the 3-split transaction (different amounts)
        var updateRes = await client.PutAsJsonAsync($"/api/v1/transactions/{txId}/multi-split", new
        {
            id = txId,
            bookedOn = "2026-01-16",
            description = "Mortgage Payment January (Corrected)",
            payeeId = (Guid?)null,
            splits = new[]
            {
                new { accountId = checking.Id, amountCents = -125000, memo = "Corrected payment" },
                new { accountId = mortgagePrincipal.Id, amountCents = 85000, memo = "Principal" },
                new { accountId = interestExpense.Id, amountCents = 40000, memo = "Interest" }
            }
        });

        updateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getRes = await client.GetAsync($"/api/v1/transactions/{txId}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getRes.Content.ReadFromJsonAsync<TransactionDto>();
        updated.Should().NotBeNull();
        updated!.Id.Should().Be(txId);
        updated.BookedOn.Should().Be(new DateOnly(2026, 1, 16));
        updated.Description.Should().Be("Mortgage Payment January (Corrected)");
        updated.Splits.Should().HaveCount(3);
        updated.Splits.Sum(s => s.Amount).Should().Be(0);

        // Verify specific amounts
        updated.Splits.First(s => s.AccountId == checking.Id).Amount.Should().Be(-1250m);
        updated.Splits.First(s => s.AccountId == mortgagePrincipal.Id).Amount.Should().Be(850m);
        updated.Splits.First(s => s.AccountId == interestExpense.Id).Amount.Should().Be(400m);
    }

    [Fact]
    public async Task MultiSplit_Update_Rejects_Unbalanced_Transaction()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var checking = await TestHelpers.CreateAccountAsync(client, "Checking", "Asset", "Checking");
        var expense1 = await TestHelpers.CreateAccountAsync(client, "Expense1", "Expense", "Other");
        var expense2 = await TestHelpers.CreateAccountAsync(client, "Expense2", "Expense", "Other");

        // Create valid 3-split transaction
        var createRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Split Payment",
            splits = new[]
            {
                new { accountId = checking.Id, amountCents = -10000, memo = (string?)null },
                new { accountId = expense1.Id, amountCents = 6000, memo = (string?)null },
                new { accountId = expense2.Id, amountCents = 4000, memo = (string?)null }
            }
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createRes.Content.ReadFromJsonAsync<TransactionDto>();
        var txId = created!.Id;

        // Try to update with unbalanced splits
        var updateRes = await client.PutAsJsonAsync($"/api/v1/transactions/{txId}/multi-split", new
        {
            id = txId,
            bookedOn = "2026-01-16",
            description = "Unbalanced Update",
            payeeId = (Guid?)null,
            splits = new[]
            {
                new { accountId = checking.Id, amountCents = -10000, memo = (string?)null },
                new { accountId = expense1.Id, amountCents = 6000, memo = (string?)null },
                new { accountId = expense2.Id, amountCents = 5000, memo = (string?)null } // Total = 1000, not 0!
            }
        });

        updateRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
