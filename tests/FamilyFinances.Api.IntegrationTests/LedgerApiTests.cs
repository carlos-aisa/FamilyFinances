using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests;

public sealed class LedgerApiTests
{
    [Fact]
    public async Task Ping_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = TestClient.CreateClient(factory);

        var res = await client.GetAsync("/api/v1/ping");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);    }

    [Fact]
    public async Task Can_Create_And_List_Accounts_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var createRes = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Main Bank",
            nature = 1, // Asset
            kind = 1,   // Checking
            openedOn = "2026-01-02"
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createRes.Content.ReadFromJsonAsync<AccountDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        var listRes = await client.GetAsync("/api/v1/accounts");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listRes.Content.ReadFromJsonAsync<List<AccountDto>>();
        list.Should().NotBeNull();
        list!.Any(a => a.Id == created.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Creating_Unbalanced_Transaction_Returns_400()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await CreateAccountAsync(client, "Groceries", "Expense", "Other");

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

        var bank = await CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var groceries = await CreateAccountAsync(client, "Groceries", "Expense", "Other");

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

    private static async Task<AccountDto> CreateAccountAsync(HttpClient client, string name, string nature, string kind)
    {
        // Convert enum names to numeric values for JSON
        var natureValue = nature switch
        {
            "Asset" => 1,
            "Liability" => 2,
            "Equity" => 3,
            "Revenue" => 4,
            "Expense" => 5,
            _ => throw new ArgumentException($"Unknown nature: {nature}")
        };

        var kindValue = kind switch
        {
            "Checking" => 1,
            "Savings" => 2,
            "CreditCard" => 3,
            "Cash" => 4,
            "Investment" => 5,
            "Loan" => 6,
            "Other" => 7,
            _ => throw new ArgumentException($"Unknown kind: {kind}")
        };

        var res = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name,
            nature = natureValue,
            kind = kindValue,
            openedOn = "2026-01-02"
        });

        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<AccountDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private sealed record AccountDto(Guid Id, string Name);

    private sealed record TransactionDto(
        Guid Id,
        string Description,
        List<TransactionSplitDto> Splits);

    private sealed record TransactionSplitDto(
        Guid AccountId,
        long AmountCents,
        string? Memo);

    private sealed record ErrorResponse(string Error);
}
