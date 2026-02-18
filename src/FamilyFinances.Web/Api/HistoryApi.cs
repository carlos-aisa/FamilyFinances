using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Web.Auth;

namespace FamilyFinances.Web.Api;

public sealed class HistoryApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public HistoryApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<FiscalYearStatusDto>> ListFiscalYearsAsync(CancellationToken ct)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "api/v1/fiscal-years");
        var response = await _http.SendAsync(request, ct);
        await EnsureAuthorizedAsync(response);
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<FiscalYearStatusDto>>(cancellationToken: ct);
        return items ?? [];
    }

    public async Task<FiscalYearStatusDto> CloseYearAsync(int year, CancellationToken ct)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"api/v1/fiscal-years/{year}/close");
        var response = await _http.SendAsync(request, ct);
        await EnsureAuthorizedAsync(response);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<FiscalYearStatusDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task<FiscalYearStatusDto> ReopenYearAsync(int year, CancellationToken ct)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"api/v1/fiscal-years/{year}/reopen");
        var response = await _http.SendAsync(request, ct);
        await EnsureAuthorizedAsync(response);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<FiscalYearStatusDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    public async Task<IReadOnlyList<TransactionListItemDto>> ListHistoricalTransactionsAsync(
        int year,
        int take,
        CancellationToken ct)
    {
        var safeTake = take < 1 ? 200 : Math.Min(take, 1000);
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/v1/history/transactions?year={year}&take={safeTake}");

        var response = await _http.SendAsync(request, ct);
        await EnsureAuthorizedAsync(response);
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<TransactionListItemDto>>(cancellationToken: ct);
        return items ?? [];
    }

    public async Task<AccountMovementsDto> GetHistoricalMovementsAsync(
        Guid accountId,
        int year,
        string? searchQuery,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 50 : Math.Min(pageSize, 100);
        var url = $"api/v1/history/movements?accountId={accountId}&year={year}&page={safePage}&pageSize={safePageSize}";
        if (!string.IsNullOrWhiteSpace(searchQuery))
            url += $"&q={Uri.EscapeDataString(searchQuery)}";

        using var request = CreateAuthorizedRequest(HttpMethod.Get, url);
        var response = await _http.SendAsync(request, ct);
        await EnsureAuthorizedAsync(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException(await ReadErrorAsync(response, ct));

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<AccountMovementsDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureAuthorizedAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        await Task.CompletedTask;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: ct);
            if (payload is not null && payload.TryGetValue("error", out var error) && !string.IsNullOrWhiteSpace(error))
                return error;
        }
        catch
        {
            // fallback below
        }

        var raw = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrWhiteSpace(raw)
            ? $"Request failed with status {(int)response.StatusCode} ({response.StatusCode})."
            : raw;
    }
}
