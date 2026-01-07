using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Web.Auth;

namespace FamilyFinances.Web.Api;

public sealed class TransactionsApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public TransactionsApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<TransactionDto>> ListAsync(int take, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/transactions?take={take}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<TransactionDto>>(cancellationToken: ct);
        return items ?? Array.Empty<TransactionDto>();
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest requestBody, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/transactions")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TransactionDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }
}
