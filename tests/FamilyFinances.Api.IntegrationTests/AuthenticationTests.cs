using System.Net;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task Ping_RequiresAuth()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = TestClient.CreateClient(factory);

        var res = await client.GetAsync("/api/v1/ping");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ping_Succeeds_WhenAuthorized()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var res = await client.GetAsync("/api/v1/ping");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
