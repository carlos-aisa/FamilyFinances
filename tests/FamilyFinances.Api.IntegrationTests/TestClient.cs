using System.Net.Http.Headers;

namespace FamilyFinances.Api.IntegrationTests;

public static class TestClient
{
    public static CustomWebApplicationFactory CreateFactoryWithFreshDb(out string dbPath)
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"familyfinances-tests-{Guid.NewGuid():N}.db");
        return new CustomWebApplicationFactory(dbPath);
    }

    public static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        return factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    public static async Task<HttpClient> CreateAuthorizedClientAsync(CustomWebApplicationFactory factory)
    {
        var client = CreateClient(factory);
        var token = await TestAuth.LoginAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
