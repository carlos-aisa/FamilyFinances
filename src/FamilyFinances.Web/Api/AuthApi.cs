using System.Net;
using System.Net.Http.Json;

namespace FamilyFinances.Web.Api;

public sealed class AuthApi
{
    private readonly HttpClient _http;

    public AuthApi(IHttpClientFactory factory)
        => _http = factory.CreateClient("FamilyFinancesApi");

    public async Task<string> LoginAsync(string email, string password, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/auth/login",
            new LoginRequest(email, password),
            ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException("Invalid credentials.");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            throw new InvalidOperationException("Missing accessToken in response.");

        return payload.AccessToken;
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed class LoginResponse
    {
        public string AccessToken { get; set; } = "";
    }
}
