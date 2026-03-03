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
        DateOnly from,
        DateOnly to,
        Guid? accountId = null,
        Guid? payeeId = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/monthly-summary?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
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

    public async Task<ReportingParetoInsightsDto> GetParetoInsightsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        ReportingInsightDimension dimension,
        int topN = 5,
        Guid? accountId = null,
        Guid? payeeId = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/insights/pareto?from={fromInclusive:yyyy-MM-dd}&to={toExclusive:yyyy-MM-dd}&dimension={ToDimensionQueryValue(dimension)}&topN={topN}";

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

        var result = await response.Content.ReadFromJsonAsync<ReportingParetoInsightsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize reporting pareto insights response.");
    }

    public async Task<ReportingAnomalyInsightsDto> GetAnomalyInsightsAsync(
        int year,
        int month,
        AccountNature nature,
        ReportingInsightDimension dimension,
        int lookbackMonths = 12,
        int requiredHistoryMonths = 3,
        Guid? accountId = null,
        Guid? payeeId = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/insights/anomalies?year={year}&month={month}&nature={nature}&dimension={ToDimensionQueryValue(dimension)}&lookbackMonths={lookbackMonths}&requiredHistoryMonths={requiredHistoryMonths}";

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

        var result = await response.Content.ReadFromJsonAsync<ReportingAnomalyInsightsDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize reporting anomaly insights response.");
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

    public async Task<AssetTotalBalanceDto> GetAssetTotalBalanceAsync(
        DateOnly asOf,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/asset-total-balance?asOf={asOf:yyyy-MM-dd}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AssetTotalBalanceDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize asset total balance response.");
    }

    public async Task<EconomicStateDto> GetEconomicStateAsync(
        DateOnly asOf,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/economic-state?asOf={asOf:yyyy-MM-dd}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EconomicStateDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize economic state response.");
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(
        int? year = null,
        int? month = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = "api/v1/reports/dashboard-overview";
        if (year is not null || month is not null)
        {
            if (year is null || month is null)
                throw new ArgumentException("Year and month must be provided together.");

            url += $"?year={year.Value}&month={month.Value}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DashboardOverviewDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize dashboard overview response.");
    }

    public async Task<MonthlyEvolutionReportDto> GetMonthlyEvolutionAsync(
        int year,
        MonthlyEvolutionScope scope,
        CancellationToken ct = default)
    {
        return await GetStateEvolutionAsync(year, scope, ct);
    }

    public async Task<MonthlyEvolutionReportDto> GetStateEvolutionAsync(
        int year,
        MonthlyEvolutionScope scope,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var scopeQueryValue = scope switch
        {
            MonthlyEvolutionScope.Accounts => "accounts",
            MonthlyEvolutionScope.AssetTotal => "asset-total",
            MonthlyEvolutionScope.AccountGroups => "account-groups",
            MonthlyEvolutionScope.IncomeTotal => "income-total",
            MonthlyEvolutionScope.ExpenseTotal => "expense-total",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported monthly evolution scope.")
        };

        var url = $"api/v1/reports/state-evolution?year={year}&scope={scopeQueryValue}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MonthlyEvolutionReportDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize monthly evolution response.");
    }

    public async Task<MonthlyBalanceChartDto> GetMonthlyChartBalanceAsync(
        int year,
        int month,
        Guid? accountId = null,
        Guid? payeeId = null,
        AccountNature? nature = null,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/monthly-charts/balance?year={year}&month={month}";
        if (accountId.HasValue)
            url += $"&accountId={accountId.Value}";
        if (payeeId.HasValue)
            url += $"&payeeId={payeeId.Value}";
        if (nature.HasValue)
            url += $"&nature={nature.Value}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MonthlyBalanceChartDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize monthly balance chart response.");
    }

    public async Task<MonthlyBalanceVsGroupsChartDto> GetMonthlyChartGroupEvolutionAsync(
        int year,
        int month,
        CancellationToken ct = default)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        var url = $"api/v1/reports/monthly-charts/group-evolution?year={year}&month={month}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MonthlyBalanceVsGroupsChartDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Failed to deserialize monthly balance vs groups chart response.");
    }

    // Legacy client alias kept for compatibility.
    public Task<MonthlyBalanceVsGroupsChartDto> GetMonthlyChartBalanceVsGroupsAsync(
        int year,
        int month,
        CancellationToken ct = default)
    {
        return GetMonthlyChartGroupEvolutionAsync(year, month, ct);
    }

    private static string ToDimensionQueryValue(ReportingInsightDimension dimension)
    {
        return dimension switch
        {
            ReportingInsightDimension.Group => "group",
            ReportingInsightDimension.Payee => "payee",
            _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Unsupported reporting insight dimension.")
        };
    }
}
