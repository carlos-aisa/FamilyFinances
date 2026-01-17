using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Web.Auth;

namespace FamilyFinances.Web.Api;

public sealed class ReportsApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public ReportsApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        int year,
        int month,
        Guid? accountId = null,
        Guid? payeeId = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/monthly-summary?year={year}&month={month}";
        if (accountId.HasValue)
            url += $"&accountId={accountId.Value}";
        if (payeeId.HasValue)
            url += $"&payeeId={payeeId.Value}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize monthly summary response.");
    }

    public async Task<CategoryTotalsDto> GetCategoryTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        Guid? payeeId = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/category-totals?from={fromInclusive:yyyy-MM-dd}&to={toExclusive:yyyy-MM-dd}&nature={nature}";
        if (payeeId.HasValue)
            url += $"&payeeId={payeeId.Value}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CategoryTotalsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize category totals response.");
    }

    public async Task<AccountTotalsDto> GetAccountTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        bool includeZeroAccounts = false,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/account-totals?from={fromInclusive:yyyy-MM-dd}&to={toExclusive:yyyy-MM-dd}&includeZeroAccounts={includeZeroAccounts}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AccountTotalsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize account totals response.");
    }

    public async Task<AccountGroupTotalsDto> GetAccountGroupTotalsAsync(
        Guid groupId,
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature? nature = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/account-groups/{groupId}/totals?from={fromInclusive:yyyy-MM-dd}&to={toExclusive:yyyy-MM-dd}";
        if (nature.HasValue)
            url += $"&nature={nature.Value}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AccountGroupTotalsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize account group totals response.");
    }
}
