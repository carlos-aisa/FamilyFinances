using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Reporting;

public sealed class AccountGroupTotalsApiTests
{
    [Fact]
    public async Task Totals_Defaults_To_Expense_When_Nature_Is_Not_Provided()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Bank", "Asset", "Checking");
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");

        var group = await CreateGroupAsync(client, "Home", "Home expenses");
        await AddAccountToGroupAsync(client, group.Id, groceries.Id);

        // Jan expense
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-10",
            description = "Groceries",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = 20_000, memo = "Payment" },
                new { accountId = groceries.Id, amountCents = -20_000, memo = "Expense" }
            }
        })).EnsureSuccessStatusCode();

        var res = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/totals?from=2026-01-01&to=2026-02-01");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupTotalsDto>();
        dto.Should().NotBeNull();
        dto!.Nature.Should().Be(AccountNature.Expense);
        dto.TotalCents.Should().Be(20_000);
        dto.TransactionsCount.Should().Be(1);
        dto.Items.Should().ContainSingle(i => i.AccountId == groceries.Id && i.TotalCents == 20_000);
    }

    [Fact]
    public async Task Totals_Can_Be_Requested_For_Income()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var bank = await TestHelpers.CreateAccountAsync(client, "Main Bank", "Asset", "Checking");
        var income = await TestHelpers.CreateAccountAsync(client, "Salary", "Income", "Other");

        var group = await CreateGroupAsync(client, "Carlos income", null);
        await AddAccountToGroupAsync(client, group.Id, income.Id);

        // Jan income
        (await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            bookedOn = "2026-01-05",
            description = "Salary",
            splits = new[]
            {
                new { accountId = bank.Id, amountCents = -100_000, memo = "Salary in" },
                new { accountId = income.Id, amountCents = 100_000, memo = "Income" }
            }
        })).EnsureSuccessStatusCode();

        var res = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/totals?from=2026-01-01&to=2026-02-01&nature=Income");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupTotalsDto>();
        dto.Should().NotBeNull();

        dto!.TotalCents.Should().Be(100_000);
        dto.TransactionsCount.Should().Be(1);
        dto.Items.Should().ContainSingle(i => i.AccountId == income.Id && i.TotalCents == 100_000);
    }

    [Fact]
    public async Task Totals_For_Group_With_No_Members_Returns_Zeroes()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var group = await CreateGroupAsync(client, "Empty group", "No accounts yet");

        var res = await client.GetAsync(
            $"/api/v1/reports/account-groups/{group.Id}/totals?from=2026-01-01&to=2026-02-01");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupTotalsDto>();
        dto.Should().NotBeNull();

        dto!.TotalCents.Should().Be(0);
        dto.TransactionsCount.Should().Be(0);
        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Totals_Should_Require_Auth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient(); // NOT authorized

        var res = await client.GetAsync(
            "/api/v1/reports/account-groups/00000000-0000-0000-0000-000000000000/totals?from=2026-01-01&to=2026-02-01");

        res.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    private static async Task<AccountGroupDto> CreateGroupAsync(HttpClient client, string name, string? description)
    {
        var res = await client.PostAsJsonAsync("/api/v1/account-groups", new { name, description });
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<AccountGroupDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    private static async Task AddAccountToGroupAsync(HttpClient client, Guid groupId, Guid accountId)
    {
        var res = await client.PostAsync($"/api/v1/account-groups/{groupId}/accounts/{accountId}", null);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public sealed record AccountGroupDto(Guid Id, string Name, string? Description);

    public sealed record AccountGroupTotalsDto(
        Guid GroupId,
        string GroupName,
        DateOnly FromInclusive,
        DateOnly ToExclusive,
        AccountNature Nature,
        long TotalCents,
        int TransactionsCount,
        int AccountsCount,
        List<AccountGroupTotalItemDto> Items);

    public sealed record AccountGroupTotalItemDto(
        Guid AccountId,
        string AccountName,
        long TotalCents,
        int TransactionsCount);
}
