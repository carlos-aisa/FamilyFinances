using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Web.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace FamilyFinances.Web.Api;

public sealed class AccountGroupsApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public AccountGroupsApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<AccountGroupDto>> ListAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/account-groups");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountGroupDto>>(cancellationToken: ct);
        return items ?? Array.Empty<AccountGroupDto>();
    }

    public async Task<AccountGroupDto> CreateAsync(CreateAccountGroupRequest requestBody, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/account-groups")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<AccountGroupDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task<AccountGroupDetailsDto> GetByIdAsync(Guid groupId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/account-groups/{groupId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account group with ID {groupId} not found.");

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<AccountGroupDetailsDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task RenameAsync(Guid groupId, string name, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/account-groups/{groupId}/rename")
        {
            Content = JsonContent.Create(new { Name = name })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account group with ID {groupId} not found.");

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();
    }

    public async Task SetDashboardPinnedAsync(Guid groupId, bool isDashboardPinned, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/account-groups/{groupId}")
        {
            Content = JsonContent.Create(new { IsDashboardPinned = isDashboardPinned })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account group with ID {groupId} not found.");

        response.EnsureSuccessStatusCode();
    }

    public async Task AddAccountAsync(Guid groupId, Guid accountId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/account-groups/{groupId}/accounts/{accountId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAccountAsync(Guid groupId, Guid accountId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/account-groups/{groupId}/accounts/{accountId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid groupId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/account-groups/{groupId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account group with ID {groupId} not found.");

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
