using FamilyFinances.Application.Ledger.Payees.Dtos;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Web.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace FamilyFinances.Web.Api;

public sealed class PayeesApi
{
    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public PayeesApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<PayeeDto>> ListAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/payees");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<PayeeDto>>(cancellationToken: ct);
        return items ?? Array.Empty<PayeeDto>();
    }

    public async Task<PayeeDto> CreateAsync(CreatePayeeRequest requestBody, CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("No access token available.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/payees")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        // If API returns 400 with domain errors, you can surface it later.
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<PayeeDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response payload.");
    }
}
