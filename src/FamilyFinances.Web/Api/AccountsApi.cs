using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Web.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace FamilyFinances.Web.Api;

internal record ErrorResponse(string Error);

public sealed class AccountsApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public AccountsApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<AccountDto>> ListAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/accounts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountDto>>(cancellationToken: ct);
        return items ?? Array.Empty<AccountDto>();
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest requestBody, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/accounts")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response, ct);
            throw new HttpRequestException($"{errorMessage}", null, response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<AccountDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    private static async Task<string> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
            return errorResponse?.Error ?? $"Request failed with status {(int)response.StatusCode} ({response.StatusCode})";
        }
        catch
        {
            return $"Request failed with status {(int)response.StatusCode} ({response.StatusCode})";
        }
    }

    public async Task RenameAsync(Guid accountId, string name, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/accounts/{accountId}/rename")
        {
            Content = JsonContent.Create(new { name })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();
    }

    public async Task CloseAsync(Guid accountId, CancellationToken ct)
    {
        await PatchNoBodyAsync($"api/v1/accounts/{accountId}/close", ct);
    }

    public async Task ReopenAsync(Guid accountId, CancellationToken ct)
    {
        await PatchNoBodyAsync($"api/v1/accounts/{accountId}/reopen", ct);
    }

    private async Task PatchNoBodyAsync(string url, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
            if (payload is not null && payload.TryGetValue("error", out var msg) && !string.IsNullOrWhiteSpace(msg))
                return msg;
        }
        catch
        {
            // Ignore parsing issues and fallback to raw content.
        }

        var raw = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(raw) ? "Request failed." : raw;
    }

}
