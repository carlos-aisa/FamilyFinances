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

    public sealed record AccountDto(Guid Id, string Name);
}
