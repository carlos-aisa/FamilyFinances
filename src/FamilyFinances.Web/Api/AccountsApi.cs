using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Auth;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace FamilyFinances.Web.Api;

internal record ErrorResponse(string Error);

public sealed class AccountsApi : IAccountsApi
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

    public async Task<IReadOnlyList<AccountKindCatalogDto>> ListKindsAsync(bool includeInactive, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var path = includeInactive ? "api/v1/accounts/kinds?includeInactive=true" : "api/v1/accounts/kinds";
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountKindCatalogDto>>(cancellationToken: ct);
        return items ?? Array.Empty<AccountKindCatalogDto>();
    }

    public async Task<AccountKindCatalogDto> CreateKindAsync(string name, AccountNature nature, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/accounts/kinds")
        {
            Content = JsonContent.Create(new { name, nature })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response, ct);
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<AccountKindCatalogDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task SetAccountKindAsync(Guid accountId, Guid kindId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/accounts/{accountId}/kind")
        {
            Content = JsonContent.Create(new { kindId })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response, ct);
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }
    }

    public async Task SetKindActiveAsync(Guid kindId, bool isActive, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/accounts/kinds/{kindId}/active")
        {
            Content = JsonContent.Create(new { isActive })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response, ct);
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }
    }

    public async Task DeleteKindAsync(Guid kindId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/accounts/kinds/{kindId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await TryReadErrorMessageAsync(response, ct);
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }
    }

    public async Task<IReadOnlyList<AccountBalanceDto>> GetBalancesAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/accounts/balances");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountBalanceDto>>(cancellationToken: ct);
        return items ?? Array.Empty<AccountBalanceDto>();
    }

    public async Task<AccountMovementsDto> GetMovementsAsync(
        Guid accountId, 
        DateOnly? fromInclusive = null, 
        DateOnly? toExclusive = null, 
        string? searchQuery = null, 
        decimal? minAmount = null,
        decimal? maxAmount = null,
        int page = 1, 
        int pageSize = 50, 
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/accounts/{accountId}/movements";
        var queryParams = new List<string>();

        if (fromInclusive.HasValue)
            queryParams.Add($"from={fromInclusive.Value:yyyy-MM-dd}");
        if (toExclusive.HasValue)
            queryParams.Add($"to={toExclusive.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(searchQuery))
            queryParams.Add($"q={Uri.EscapeDataString(searchQuery)}");
        if (minAmount.HasValue)
            queryParams.Add($"minAmount={minAmount.Value.ToString(CultureInfo.InvariantCulture)}");
        if (maxAmount.HasValue)
            queryParams.Add($"maxAmount={maxAmount.Value.ToString(CultureInfo.InvariantCulture)}");
        if (page != 1)
            queryParams.Add($"page={page}");
        if (pageSize != 50)
            queryParams.Add($"pageSize={pageSize}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AccountMovementsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Empty response payload.");
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

    public async Task<ReconcileAccountResponse> ReconcileAsync(
        Guid accountId,
        decimal actualBalance,
        DateOnly asOfDate,
        string? note,
        CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var requestBody = new
        {
            actualBalance,
            asOfDate = asOfDate.ToString("yyyy-MM-dd"),
            note
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/accounts/{accountId}/reconcile")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage = await ReadErrorAsync(response, ct);
            throw new InvalidOperationException(errorMessage);
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ReconcileAccountResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task DeleteAsync(Guid accountId, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/accounts/{accountId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();
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
