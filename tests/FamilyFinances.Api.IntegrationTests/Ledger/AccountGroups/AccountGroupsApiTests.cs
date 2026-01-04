using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.AccountGroups;

public sealed class AccountGroupsApiTests
{
    [Fact]
    public async Task Create_And_List_Works_With_Optional_Description()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create without description
        var res1 = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Carlos expenses",
            description = (string?)null
        });
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        var g1 = await res1.Content.ReadFromJsonAsync<AccountGroupDto>();
        g1.Should().NotBeNull();
        g1!.Name.Should().Be("Carlos expenses");
        g1.Description.Should().BeNull();

        // Create with description
        var res2 = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Home fixed expenses",
            description = "Recurring household bills"
        });
        res2.StatusCode.Should().Be(HttpStatusCode.OK);

        var g2 = await res2.Content.ReadFromJsonAsync<AccountGroupDto>();
        g2.Should().NotBeNull();
        g2!.Description.Should().Be("Recurring household bills");

        // List
        var listRes = await client.GetAsync("/api/v1/account-groups");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listRes.Content.ReadFromJsonAsync<List<AccountGroupDto>>();
        list.Should().NotBeNull();
        list!.Select(x => x.Id).Should().Contain(new[] { g1.Id, g2.Id });
    }

    [Fact]
    public async Task Create_Should_Reject_Duplicate_Name_CaseInsensitive()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        (await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Fixed Bills",
            description = (string?)null
        })).StatusCode.Should().Be(HttpStatusCode.OK);

        var dup = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "fixed bills",
            description = "duplicate"
        });

        dup.IsSuccessStatusCode.Should().BeFalse();
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Add_Remove_Membership_And_GetById_Works()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Accounts
        var groceries = await TestHelpers.CreateAccountAsync(client, "Groceries", "Expense", "Other");
        var fixedBills = await TestHelpers.CreateAccountAsync(client, "Fixed Bills", "Expense", "Other");

        // Group
        var create = await client.PostAsJsonAsync("/api/v1/account-groups", new
        {
            name = "Home",
            description = "Home related expenses"
        });
        create.StatusCode.Should().Be(HttpStatusCode.OK);

        var group = await create.Content.ReadFromJsonAsync<AccountGroupDto>();
        group.Should().NotBeNull();

        // Add memberships
        (await client.PostAsync($"/api/v1/account-groups/{group!.Id}/accounts/{groceries.Id}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostAsync($"/api/v1/account-groups/{group.Id}/accounts/{fixedBills.Id}", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get details
        var get1 = await client.GetAsync($"/api/v1/account-groups/{group.Id}");
        get1.StatusCode.Should().Be(HttpStatusCode.OK);

        var details1 = await get1.Content.ReadFromJsonAsync<AccountGroupDetailsDto>();
        details1.Should().NotBeNull();
        details1!.Accounts.Select(a => a.AccountId).Should().BeEquivalentTo(new[] { groceries.Id, fixedBills.Id });

        // Remove one membership
        (await client.DeleteAsync($"/api/v1/account-groups/{group.Id}/accounts/{groceries.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Idempotent remove (remove again)
        (await client.DeleteAsync($"/api/v1/account-groups/{group.Id}/accounts/{groceries.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get again
        var get2 = await client.GetAsync($"/api/v1/account-groups/{group.Id}");
        get2.StatusCode.Should().Be(HttpStatusCode.OK);

        var details2 = await get2.Content.ReadFromJsonAsync<AccountGroupDetailsDto>();
        details2.Should().NotBeNull();
        details2!.Accounts.Select(a => a.AccountId).Should().BeEquivalentTo(new[] { fixedBills.Id });
    }

    [Fact]
    public async Task Endpoints_Should_Require_Auth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient(); // NOT authorized

        var list = await client.GetAsync("/api/v1/account-groups");
        list.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        var create = await client.PostAsJsonAsync("/api/v1/account-groups", new { name = "X", description = (string?)null });
        create.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // Minimal DTOs for deserialization
    public sealed record AccountGroupDto(Guid Id, string Name, string? Description);

    public sealed record AccountGroupDetailsDto(
        Guid Id,
        string Name,
        string? Description,
        List<AccountRefDto> Accounts);

    public sealed record AccountRefDto(
        Guid AccountId,
        string Name,
        AccountNature Nature,
        AccountKind Kind);
}
