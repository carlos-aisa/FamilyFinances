using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.FiscalYears;

public sealed class FiscalYearGovernanceApiTests
{
    [Fact]
    public async Task CloseYear_BlocksMutations_AndReopenAllowsMutations()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var savings = await TestHelpers.CreateAccountAsync(client, "Savings", "Asset", "Savings");

        var existing = await CreateTransactionAsync(client, "2025-06-10", "Existing", new[]
        {
            new { accountId = bank.Id, amountCents = 2_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -2_000, memo = "Groceries" }
        });

        var closeResponse = await client.PostAsync("/api/v1/fiscal-years/2025/close", null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var closeStatus = await closeResponse.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        closeStatus.Should().NotBeNull();
        closeStatus!.Year.Should().Be(2025);
        closeStatus.IsClosed.Should().BeTrue();

        var blockedCreate = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2025-06-11",
            description = "Blocked create",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 1_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -1_000, memo = "Groceries" }
            }
        });
        blockedCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorAsync(blockedCreate)).Should().Contain("Year 2025 is closed");

        var blockedUpdate = await client.PutAsJsonAsync($"/api/v1/transactions/{existing.Id}", new
        {
            id = existing.Id,
            bookedOn = "2025-06-10",
            description = "Blocked update",
            payeeId = (Guid?)null,
            fromAccountId = bank.Id,
            toAccountId = savings.Id,
            amount = 20m
        });
        blockedUpdate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorAsync(blockedUpdate)).Should().Contain("Year 2025 is closed");

        var blockedDelete = await client.DeleteAsync($"/api/v1/transactions/{existing.Id}");
        blockedDelete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorAsync(blockedDelete)).Should().Contain("Year 2025 is closed");

        var blockedReconcile = await client.PostAsJsonAsync($"/api/v1/accounts/{bank.Id}/reconcile", new
        {
            actualBalance = 10m,
            asOfDate = "2025-12-31",
            note = "Blocked reconcile"
        });
        blockedReconcile.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadErrorAsync(blockedReconcile)).Should().Contain("Year 2025 is closed");

        var reopenResponse = await client.PostAsync("/api/v1/fiscal-years/2025/reopen", null);
        reopenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reopenStatus = await reopenResponse.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        reopenStatus.Should().NotBeNull();
        reopenStatus!.IsClosed.Should().BeFalse();

        var allowedCreate = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2025-07-01",
            description = "Allowed create",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 1_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -1_000, memo = "Groceries" }
            }
        });
        allowedCreate.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CloseAndReopen_AreIdempotent()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var close1 = await client.PostAsync("/api/v1/fiscal-years/2024/close", null);
        var close2 = await client.PostAsync("/api/v1/fiscal-years/2024/close", null);
        close1.StatusCode.Should().Be(HttpStatusCode.OK);
        close2.StatusCode.Should().Be(HttpStatusCode.OK);

        var closeStatus1 = await close1.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        var closeStatus2 = await close2.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        closeStatus1!.IsClosed.Should().BeTrue();
        closeStatus2!.IsClosed.Should().BeTrue();

        var reopen1 = await client.PostAsync("/api/v1/fiscal-years/2024/reopen", null);
        var reopen2 = await client.PostAsync("/api/v1/fiscal-years/2024/reopen", null);
        reopen1.StatusCode.Should().Be(HttpStatusCode.OK);
        reopen2.StatusCode.Should().Be(HttpStatusCode.OK);

        var reopenStatus1 = await reopen1.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        var reopenStatus2 = await reopen2.Content.ReadFromJsonAsync<FiscalYearStatusDto>();
        reopenStatus1!.IsClosed.Should().BeFalse();
        reopenStatus2!.IsClosed.Should().BeFalse();
    }

    [Fact]
    public async Task ListFiscalYears_ReturnsStatusMetadata()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var close = await client.PostAsync("/api/v1/fiscal-years/2025/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);

        var reopen = await client.PostAsync("/api/v1/fiscal-years/2025/reopen", null);
        reopen.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetAsync("/api/v1/fiscal-years");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        var statuses = await list.Content.ReadFromJsonAsync<List<FiscalYearStatusDto>>();
        statuses.Should().NotBeNull();
        statuses.Should().NotBeEmpty();

        var year2025 = statuses!.Single(x => x.Year == 2025);
        year2025.IsClosed.Should().BeFalse();
        year2025.ClosedAtUtc.Should().NotBeNull();
        year2025.ReopenedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task HistoricalEndpoints_FilterByYear_AndReturnRunningBalances()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await CreateTransactionAsync(client, "2024-12-31", "Salary 2024", new[]
        {
            new { accountId = salary.Id, amountCents = 10_000, memo = "Salary" },
            new { accountId = bank.Id, amountCents = -10_000, memo = "Into bank" }
        });

        await CreateTransactionAsync(client, "2025-01-10", "Groceries 1", new[]
        {
            new { accountId = bank.Id, amountCents = 3_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -3_000, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2025-01-11", "Groceries 2", new[]
        {
            new { accountId = bank.Id, amountCents = 2_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -2_000, memo = "Expense" }
        });

        var close = await client.PostAsync("/api/v1/fiscal-years/2024/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);

        var txHistory = await client.GetAsync("/api/v1/history/transactions?year=2025&take=100");
        txHistory.StatusCode.Should().Be(HttpStatusCode.OK);
        var txItems = await txHistory.Content.ReadFromJsonAsync<List<HistoricalTransactionDto>>();
        txItems.Should().NotBeNull();
        txItems!.Should().OnlyContain(x => x.BookedOn.Year == 2025);
        txItems.Should().HaveCount(2);

        var movementsHistory = await client.GetAsync($"/api/v1/history/movements?accountId={bank.Id}&year=2025&page=1&pageSize=50");
        movementsHistory.StatusCode.Should().Be(HttpStatusCode.OK);
        var movements = await movementsHistory.Content.ReadFromJsonAsync<AccountMovementsDto>();
        movements.Should().NotBeNull();
        movements!.Items.Should().HaveCount(2);

        var groceries1 = movements.Items.Single(x => x.Description == "Groceries 1");
        var groceries2 = movements.Items.Single(x => x.Description == "Groceries 2");

        groceries1.RunningBalance.Should().Be(-70.00m);
        groceries2.RunningBalance.Should().Be(-50.00m);
    }

    [Fact]
    public async Task AccountMovements_ParityIsMaintained_BeforeAndAfterSnapshotClose()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        await CreateTransactionAsync(client, "2024-12-31", "Salary 2024", new[]
        {
            new { accountId = salary.Id, amountCents = 8_000, memo = "Salary" },
            new { accountId = bank.Id, amountCents = -8_000, memo = "Into bank" }
        });

        await CreateTransactionAsync(client, "2025-01-05", "Groceries A", new[]
        {
            new { accountId = bank.Id, amountCents = 1_500, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -1_500, memo = "Expense" }
        });

        await CreateTransactionAsync(client, "2025-01-12", "Groceries B", new[]
        {
            new { accountId = bank.Id, amountCents = 2_500, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = -2_500, memo = "Expense" }
        });

        var beforeClose = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2025-01-01&to=2026-01-01&page=1&pageSize=50");
        beforeClose.StatusCode.Should().Be(HttpStatusCode.OK);
        var beforePayload = await beforeClose.Content.ReadFromJsonAsync<AccountMovementsDto>();
        beforePayload.Should().NotBeNull();

        var close = await client.PostAsync("/api/v1/fiscal-years/2024/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterClose = await client.GetAsync(
            $"/api/v1/accounts/{bank.Id}/movements?from=2025-01-01&to=2026-01-01&page=1&pageSize=50");
        afterClose.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterPayload = await afterClose.Content.ReadFromJsonAsync<AccountMovementsDto>();
        afterPayload.Should().NotBeNull();

        var beforeItems = beforePayload!.Items.Select(x => new { x.TransactionId, x.SignedAmount, x.RunningBalance }).ToList();
        var afterItems = afterPayload!.Items.Select(x => new { x.TransactionId, x.SignedAmount, x.RunningBalance }).ToList();

        afterItems.Should().BeEquivalentTo(beforeItems, options => options.WithStrictOrdering());
    }

    private static async Task<TransactionDto> CreateTransactionAsync(
        HttpClient client,
        string bookedOn,
        string description,
        object[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn,
            description,
            splits
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<TransactionDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return payload?.Error ?? string.Empty;
    }

    public sealed record FiscalYearStatusDto(
        int Year,
        bool IsClosed,
        DateTime? ClosedAtUtc,
        string? ClosedByUserId,
        DateTime? ReopenedAtUtc,
        string? ReopenedByUserId);

    public sealed record ErrorResponse(string Error);

    public sealed record TransactionDto(Guid Id);

    public sealed record HistoricalTransactionDto(
        Guid Id,
        DateOnly BookedOn,
        string Headline,
        string? Subheadline,
        decimal Amount,
        int Type);

    public sealed record AccountMovementsDto(
        Guid AccountId,
        string AccountName,
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        List<AccountMovementDto> Items,
        int TotalCount);

    public sealed record AccountMovementDto(
        Guid TransactionId,
        DateOnly BookedOn,
        string Description,
        string? PayeeName,
        decimal SignedAmount,
        string? CounterpartyAccountName,
        decimal RunningBalance);
}
