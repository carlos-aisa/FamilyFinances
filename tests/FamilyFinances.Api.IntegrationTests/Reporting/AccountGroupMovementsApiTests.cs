using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Api.IntegrationTests.Helpers;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class AccountGroupMovementsApiTests
{
    [Fact]
    public async Task Movements_ReturnsOneRowPerTransaction_WithMultiSplitContext_AndRunningNet()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var transport = await TestHelpers.CreateAccountAsync(client, "Transport", "Expense", "Other");
        var group = await CreateGroupAsync(client, "Household expenses");
        await AddAccountToGroupAsync(client, group.Id, groceries.Id);
        await AddAccountToGroupAsync(client, group.Id, transport.Id);

        await CreateTransactionAsync(client, "2026-01-10", "Weekly shopping", new[]
        {
            new { accountId = bank.Id, amountCents = -5_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = 3_000, memo = "Food" },
            new { accountId = transport.Id, amountCents = 2_000, memo = "Travel" }
        });
        await CreateTransactionAsync(client, "2026-01-15", "Envelope transfer", new[]
        {
            new { accountId = groceries.Id, amountCents = -1_000, memo = "Move out" },
            new { accountId = transport.Id, amountCents = 1_000, memo = "Move in" }
        });
        await CreateTransactionAsync(client, "2026-01-20", "Store refund", new[]
        {
            new { accountId = groceries.Id, amountCents = -1_000, memo = "Refund" },
            new { accountId = bank.Id, amountCents = 1_000, memo = "Refund received" }
        });

        var response = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/movements?from=2026-01-01&to=2026-02-01&nature=Expense");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AccountGroupMovementsDto>();
        result.Should().NotBeNull();
        result!.Nature.Should().Be(AccountNature.Expense);
        result.Items.Select(item => item.Description).Should().Equal("Store refund", "Envelope transfer", "Weekly shopping");

        var refund = result.Items[0];
        refund.SourceAccountNames.Should().Equal("Groceries");
        refund.DestinationAccountNames.Should().Equal("Bank");
        refund.GroupNetCents.Should().Be(1_000);
        refund.RunningNetCents.Should().Be(-4_000);

        var internalTransfer = result.Items[1];
        internalTransfer.SourceAccountNames.Should().Equal("Groceries");
        internalTransfer.DestinationAccountNames.Should().Equal("Transport");
        internalTransfer.GroupNetCents.Should().Be(0);
        internalTransfer.RunningNetCents.Should().Be(-5_000);

        var purchase = result.Items[2];
        purchase.SourceAccountNames.Should().Equal("Bank");
        purchase.DestinationAccountNames.Should().Equal("Groceries", "Transport");
        purchase.GroupNetCents.Should().Be(-5_000);
        purchase.RunningNetCents.Should().Be(-5_000);
    }

    [Fact]
    public async Task Movements_AppliesDateAndNatureFilters_ToGroupSplits()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var salary = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");
        var group = await CreateGroupAsync(client, "Mixed group");
        await AddAccountToGroupAsync(client, group.Id, groceries.Id);
        await AddAccountToGroupAsync(client, group.Id, salary.Id);

        await CreateTransactionAsync(client, "2026-01-10", "Food", new[]
        {
            new { accountId = bank.Id, amountCents = -2_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = 2_000, memo = "Expense" }
        });
        await CreateTransactionAsync(client, "2026-01-20", "Salary", new[]
        {
            new { accountId = bank.Id, amountCents = 10_000, memo = "Received" },
            new { accountId = salary.Id, amountCents = -10_000, memo = "Income" }
        });
        await CreateTransactionAsync(client, "2026-02-01", "Later food", new[]
        {
            new { accountId = bank.Id, amountCents = -3_000, memo = "Payment" },
            new { accountId = groceries.Id, amountCents = 3_000, memo = "Expense" }
        });

        var response = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/movements?from=2026-01-01&to=2026-02-01&nature=Income");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AccountGroupMovementsDto>();
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].Description.Should().Be("Salary");
        result.Items[0].GroupNetCents.Should().Be(10_000);
        result.Items[0].RunningNetCents.Should().Be(10_000);
    }

    [Fact]
    public async Task Movements_ReturnsNotFoundForMissingGroup_AndRequiresAuthorization()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var authorizedClient = await TestClient.CreateAuthorizedClientAsync(factory);
        using var anonymousClient = factory.CreateClient();
        var groupId = Guid.NewGuid();

        var missingResponse = await authorizedClient.GetAsync(
            $"/api/v1/reports/account-groups/{groupId}/movements?from=2026-01-01&to=2026-02-01");
        var anonymousResponse = await anonymousClient.GetAsync(
            $"/api/v1/reports/account-groups/{groupId}/movements?from=2026-01-01&to=2026-02-01");

        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        anonymousResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    private static async Task CreateTransactionAsync(HttpClient client, string bookedOn, string description, object[] splits)
    {
        var response = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn,
            description,
            splits
        });

        response.EnsureSuccessStatusCode();
    }

    private static async Task<AccountGroupDto> CreateGroupAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/account-groups", new { name });
        response.EnsureSuccessStatusCode();

        var group = await response.Content.ReadFromJsonAsync<AccountGroupDto>();
        group.Should().NotBeNull();
        return group!;
    }

    private static async Task AddAccountToGroupAsync(HttpClient client, Guid groupId, Guid accountId)
    {
        var response = await client.PostAsync($"/api/v1/account-groups/{groupId}/accounts/{accountId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public sealed record AccountGroupDto(Guid Id, string Name, string? Description);

    public sealed record AccountGroupMovementsDto(
        Guid GroupId,
        string GroupName,
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        AccountNature? Nature,
        List<AccountGroupMovementDto> Items);

    public sealed record AccountGroupMovementDto(
        Guid TransactionId,
        DateOnly BookedOn,
        string Description,
        string? PayeeName,
        List<string> SourceAccountNames,
        List<string> DestinationAccountNames,
        long GroupNetCents,
        long RunningNetCents);
}
