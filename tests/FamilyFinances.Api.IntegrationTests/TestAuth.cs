using System.Net.Http.Json;

namespace FamilyFinances.Api.IntegrationTests;

public static class TestAuth
{
    public static async Task<string> LoginAndGetTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@familyfinances.local",
            password = "Admin123!"
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            throw new InvalidOperationException("Login did not return an access token.");

        return payload.AccessToken;
    }

    private sealed record LoginResponse(string AccessToken);
}
