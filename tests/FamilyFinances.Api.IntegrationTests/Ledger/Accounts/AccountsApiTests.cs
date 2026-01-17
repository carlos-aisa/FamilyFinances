using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FamilyFinances.Api.IntegrationTests.Helpers;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Accounts;

public sealed class AccountsApiTests
{
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
    public async Task Create_Account_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Bank",
            nature = 1,
            kind = 1,
            openedOn = "2026-01-02"
        });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_Accounts_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.GetAsync("/api/v1/accounts");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Rename_Account_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create an account first
        var createRes = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Original Name",
            nature = 1, // Asset
            kind = 1,   // Checking
            openedOn = "2026-01-02"
        });
        var created = await createRes.Content.ReadFromJsonAsync<AccountDto>();

        // Rename the account
        var renameRes = await client.PatchAsJsonAsync($"/api/v1/accounts/{created!.Id}/rename", new
        {
            name = "New Name"
        });

        renameRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the name changed
        var listRes = await client.GetAsync("/api/v1/accounts");
        var list = await listRes.Content.ReadFromJsonAsync<List<AccountDto>>();
        var renamed = list!.FirstOrDefault(a => a.Id == created.Id);
        renamed.Should().NotBeNull();
        renamed!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Rename_Account_Returns404_WhenNotFound()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var renameRes = await client.PatchAsJsonAsync($"/api/v1/accounts/{Guid.NewGuid()}/rename", new
        {
            name = "New Name"
        });

        renameRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Rename_Account_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.PatchAsJsonAsync($"/api/v1/accounts/{Guid.NewGuid()}/rename", new
        {
            name = "New Name"
        });

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Close_Account_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create an account first
        var createRes = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Account to Close",
            nature = 1, // Asset
            kind = 1,   // Checking
            openedOn = "2026-01-02"
        });
        var created = await createRes.Content.ReadFromJsonAsync<AccountDto>();

        // Close the account
        var closeRes = await client.PatchAsync($"/api/v1/accounts/{created!.Id}/close", null);

        closeRes.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Close_Account_Returns404_WhenNotFound()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var closeRes = await client.PatchAsync($"/api/v1/accounts/{Guid.NewGuid()}/close", null);

        closeRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Close_Account_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.PatchAsync($"/api/v1/accounts/{Guid.NewGuid()}/close", null);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Reopen_Account_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create and close an account first
        var createRes = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Account to Reopen",
            nature = 1, // Asset
            kind = 1,   // Checking
            openedOn = "2026-01-02"
        });
        var created = await createRes.Content.ReadFromJsonAsync<AccountDto>();
        await client.PatchAsync($"/api/v1/accounts/{created!.Id}/close", null);

        // Reopen the account
        var reopenRes = await client.PatchAsync($"/api/v1/accounts/{created.Id}/reopen", null);

        reopenRes.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Reopen_Account_Returns404_WhenNotFound()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var reopenRes = await client.PatchAsync($"/api/v1/accounts/{Guid.NewGuid()}/reopen", null);

        reopenRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reopen_Account_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.PatchAsync($"/api/v1/accounts/{Guid.NewGuid()}/reopen", null);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Delete_Account_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create an account
        var createRes = await client.PostAsJsonAsync("/api/v1/accounts", new
        {
            name = "Account to Delete",
            nature = 1, // Asset
            kind = 1,   // Checking
            openedOn = "2026-01-02"
        });
        var created = await createRes.Content.ReadFromJsonAsync<AccountDto>();
        created.Should().NotBeNull();

        // Delete the account
        var deleteRes = await client.DeleteAsync($"/api/v1/accounts/{created!.Id}");
        deleteRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's not in the list anymore
        var listRes = await client.GetAsync("/api/v1/accounts");
        var list = await listRes.Content.ReadFromJsonAsync<List<AccountDto>>();
        list.Should().NotBeNull();
        list!.Should().NotContain(a => a.Id == created.Id);
    }

    [Fact]
    public async Task Delete_Account_Returns404_WhenNotFound()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var deleteRes = await client.DeleteAsync($"/api/v1/accounts/{Guid.NewGuid()}");

        deleteRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Account_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = factory.CreateClient();

        var res = await client.DeleteAsync($"/api/v1/accounts/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public sealed record AccountDto(Guid Id, string Name);
}
