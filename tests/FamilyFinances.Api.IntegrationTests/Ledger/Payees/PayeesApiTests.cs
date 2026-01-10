using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Ledger.Payees;

public sealed class PayeesApiTests
{
    [Fact]
    public async Task ListPayees_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = TestClient.CreateClient(factory);

        var res = await client.GetAsync("/api/v1/payees");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Create_And_List_Payees_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create
        var createRes = await client.PostAsJsonAsync("/api/v1/payees", new
        {
            name = "  Netflix  "
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createRes.Content.ReadFromJsonAsync<PayeeDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Netflix");

        // List
        var listRes = await client.GetAsync("/api/v1/payees");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var payees = await listRes.Content.ReadFromJsonAsync<List<PayeeDto>>();
        payees.Should().NotBeNull();
        payees!.Should().ContainSingle(p => p.Id == created.Id && p.Name == "Netflix");
    }

    [Fact]
    public async Task CreatePayee_ReturnsBadRequest_WhenNameAlreadyExists_CaseInsensitive()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var first = await client.PostAsJsonAsync("/api/v1/payees", new { name = "Netflix" });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var dup = await client.PostAsJsonAsync("/api/v1/payees", new { name = "netflix" });
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await dup.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Can_Rename_Payee()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        // Create payee
        var createRes = await client.PostAsJsonAsync("/api/v1/payees", new { name = "Amazon" });
        createRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createRes.Content.ReadFromJsonAsync<PayeeDto>();
        created.Should().NotBeNull();

        // Rename - test if it works properly with lowercase (camelCase)
        var renameRes = await client.PatchAsJsonAsync($"/api/v1/payees/{created!.Id}/rename", new { name = "AWS" });
        var renameContent = await renameRes.Content.ReadAsStringAsync();
        Console.WriteLine($"Rename response: {renameRes.StatusCode} - {renameContent}");
        renameRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // List to verify the name changed
        var listRes = await client.GetAsync("/api/v1/payees");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var payees = await listRes.Content.ReadFromJsonAsync<List<PayeeDto>>();
        
        Console.WriteLine($"Payees after rename: {string.Join(", ", payees!.Select(p => $"{p.Id}:{p.Name}"))}");
        
        payees.Should().NotBeNull();
        payees!.Should().ContainSingle(p => p.Id == created.Id && p.Name == "AWS");
    }

    private sealed record PayeeDto(Guid Id, string Name);

    private sealed record ErrorResponse(string Error);
}
