using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Transactions;

public sealed class RefundsApiTests
{
    [Fact]
    public async Task Can_Create_Refund_Without_Linking()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Act - Create refund without linking (expense account decreased, asset increased)
        var refundRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-19",
            description = "Amazon refund",
            splits = new[]
            {
                new { accountId = groceries.Id, amountCents = -3300, memo = (string?)null }, // Expense decreased
                new { accountId = bank.Id, amountCents = 3300, memo = (string?)"Refund received" }    // Asset increased
            },
            payeeId = (Guid?)null,
            relatedTransactionId = (Guid?)null
        });

        // Assert
        refundRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await refundRes.Content.ReadFromJsonAsync<TransactionDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Splits.Should().HaveCount(2);
        created.Splits.Sum(s => (long)(s.Amount * 100)).Should().Be(0);

        // Verify transaction appears in list as Refund
        var listRes = await client.GetAsync("/api/v1/transactions?take=10");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listRes.Content.ReadFromJsonAsync<List<TransactionListDto>>();
        list.Should().NotBeNull();
        
        var refund = list!.FirstOrDefault(t => t.Id == created.Id);
        refund.Should().NotBeNull();
        refund!.Type.Should().Be(3); // TransactionListItemType.Refund
        refund.Headline.Should().Be("Groceries");
    }

    [Fact]
    public async Task Can_Create_Refund_With_RelatedTransactionId()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create original expense transaction
        var expenseRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Amazon purchase",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5000, memo = "Expense" }
            }
        });

        expenseRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var originalExpense = await expenseRes.Content.ReadFromJsonAsync<TransactionDto>();
        originalExpense.Should().NotBeNull();

        // Act - Create refund linked to original transaction
        var refundRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Amazon refund - wrong item",
            splits = new[]
            {
                new { accountId = groceries.Id, amountCents = -5000, memo = (string?)null },
                new { accountId = bank.Id, amountCents = 5000, memo = (string?)"Refund" }
            },
            payeeId = (Guid?)null,
            relatedTransactionId = originalExpense!.Id
        });

        // Assert
        refundRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var refund = await refundRes.Content.ReadFromJsonAsync<TransactionDto>();
        refund.Should().NotBeNull();
        refund!.Id.Should().NotBeEmpty();
        refund.Id.Should().NotBe(originalExpense.Id);
    }

    [Fact]
    public async Task Create_Refund_Fails_When_RelatedTransactionId_Does_Not_Exist()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        var nonExistentId = Guid.NewGuid();

        // Act
        var refundRes = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-15",
            description = "Invalid refund",
            splits = new[]
            {
                new { accountId = groceries.Id, amountCents = -2000, memo = (string?)null },
                new { accountId = bank.Id, amountCents = 2000, memo = (string?)"Refund" }
            },
            relatedTransactionId = nonExistentId
        });

        // Assert
        refundRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await refundRes.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Can_Search_Expenses_For_Refund_Linking()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var utilities = await TestHelpers.CreateAccountAsync(client, "Utilities", "Expense", "Other");

        // Create some expense transactions
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Amazon purchase - electronics",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -15000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 15000, memo = "Expense" }
            }
        });

        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Electric bill",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -8000, memo = "Payment" },
                new { accountId = utilities.Id, amountCents = 8000, memo = "Expense" }
            }
        });

        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-12",
            description = "Amazon purchase - books",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -2500, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 2500, memo = "Expense" }
            }
        });

        // Act - Search by description
        var searchRes = await client.GetAsync("/api/v1/transactions/search-expenses?q=Amazon&limit=20");

        // Assert
        searchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await searchRes.Content.ReadFromJsonAsync<List<ExpenseSearchResultDto>>();
        results.Should().NotBeNull();
        results!.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.Description.Contains("Amazon", StringComparison.OrdinalIgnoreCase));
        results.Should().OnlyContain(r => r.Amount > 0);
    }

    [Fact]
    public async Task Search_Expenses_Respects_Limit()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        // Create multiple expense transactions
        for (int i = 1; i <= 10; i++)
        {
            await client.PostAsJsonAsync("/api/v1/transactions", new
            {
                bookedOn = $"2026-01-{i:D2}",
                description = $"Purchase {i}",
                splits = new[]
                {
                    new { accountId = bank.Id, amountCents = -1000, memo = "Payment" },
                    new { accountId = groceries.Id, amountCents = 1000, memo = "Expense" }
                }
            });
        }

        // Act - Search with limit
        var searchRes = await client.GetAsync("/api/v1/transactions/search-expenses?q=Purchase&limit=5");

        // Assert
        searchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await searchRes.Content.ReadFromJsonAsync<List<ExpenseSearchResultDto>>();
        results.Should().NotBeNull();
        results!.Should().HaveCount(5);
    }

    [Fact]
    public async Task Search_Expenses_Returns_Empty_For_Short_Query()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Act
        var searchRes = await client.GetAsync("/api/v1/transactions/search-expenses?q=a&limit=20");

        // Assert
        searchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await searchRes.Content.ReadFromJsonAsync<List<ExpenseSearchResultDto>>();
        results.Should().NotBeNull();
        results!.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_Expenses_Requires_Auth()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        // Act
        var searchRes = await client.GetAsync("/api/v1/transactions/search-expenses?q=test&limit=20");

        // Assert
        searchRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_Expenses_Can_Filter_By_ExpenseAccountId()
    {
        // Arrange
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var utilities = await TestHelpers.CreateAccountAsync(client, "Utilities", "Expense", "Other");

        // Create expense in groceries
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Food shopping",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -5000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = 5000, memo = "Expense" }
            }
        });

        // Create expense in utilities
        await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Food truck",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -1500, memo = "Payment" },
                new { accountId = utilities.Id, amountCents = 1500, memo = "Expense" }
            }
        });

        // Act - Filter by groceries account
        var searchRes = await client.GetAsync($"/api/v1/transactions/search-expenses?q=Food&expenseAccountId={groceries.Id}&limit=20");

        // Assert
        searchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await searchRes.Content.ReadFromJsonAsync<List<ExpenseSearchResultDto>>();
        results.Should().NotBeNull();
        results!.Should().HaveCount(1);
        results[0].ExpenseAccountName.Should().Be("Groceries");
    }

    public sealed record TransactionDto(
        Guid Id,
        DateOnly BookedOn,
        string Description,
        Guid? PayeeId,
        string? PayeeName,
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

    public sealed record ExpenseSearchResultDto(
        Guid Id,
        string Description,
        DateOnly BookedOn,
        string? PayeeName,
        decimal Amount,
        string ExpenseAccountName);

    public sealed record ErrorResponse(string Error);
}
