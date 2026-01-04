namespace FamilyFinances.Web.Api
{
    public sealed class AuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(HttpClient http) => _http = http;

        public async Task<string> LoginAsync(string email, string password, CancellationToken ct)
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/login", new { email, password }, ct);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Login failed.");

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            return payload?.AccessToken ?? throw new InvalidOperationException("Missing accessToken in response.");
        }

        private sealed class LoginResponse
        {
            public string AccessToken { get; set; } = "";
        }
    }
}
