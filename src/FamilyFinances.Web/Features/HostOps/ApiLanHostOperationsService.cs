using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Web.Auth;

namespace FamilyFinances.Web.Features.HostOps;

public sealed class ApiLanHostOperationsService : ILanHostOperationsService
{
    private const string AccessTokenCookieName = "ff_access_token";

    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiLanHostOperationsService> _logger;

    public ApiLanHostOperationsService(
        IHttpClientFactory factory,
        IApiTokenStore tokenStore,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiLanHostOperationsService> logger)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, "api/v1/ops/lan/status", ct);
        using var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadErrorMessageAsync(response, ct) ?? "Failed to load LAN host status.";
            throw new InvalidOperationException(message);
        }

        var status = await response.Content.ReadFromJsonAsync<LanAccessStatus>(cancellationToken: ct);
        return status ?? throw new InvalidOperationException("Failed to deserialize LAN status response.");
    }

    public async Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
    {
        if (!LanAccessCommandValidator.IsValidPort(request.HttpsPort))
        {
            return new LanOperationResult(false, $"Invalid HTTPS port: {request.HttpsPort}.");
        }

        using var message = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/ops/lan/apply", ct);
        message.Content = JsonContent.Create(request);

        using var response = await _http.SendAsync(message, ct);
        return await ReadOperationResultAsync(response, ct);
    }

    public async Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default)
    {
        if (!LanAccessCommandValidator.IsValidPort(httpsPort))
        {
            return new LanOperationResult(false, $"Invalid HTTPS port: {httpsPort}.");
        }

        var request = new LanAccessRequest(
            Enabled: true,
            HttpsPort: httpsPort,
            HostName: hostName,
            RegenerateCertificate: true);

        using var message = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/ops/lan/certificate/regenerate", ct);
        message.Content = JsonContent.Create(request);

        using var response = await _http.SendAsync(message, ct);
        return await ReadOperationResultAsync(response, ct);
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string uri, CancellationToken ct)
    {
        var token = await GetRequiredTokenAsync(ct);
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<string> GetRequiredTokenAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();

        if (string.IsNullOrWhiteSpace(token) && _httpContextAccessor.HttpContext is not null)
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(AccessTokenCookieName, out token);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            token = await _tokenStore.WaitForTokenAsync(TimeSpan.FromSeconds(3), ct);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("No access token available.");
        }

        return token;
    }

    private async Task<LanOperationResult> ReadOperationResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new LanOperationResult(false, "API call unauthorized. Missing or invalid token.");
        }

        try
        {
            var parsed = await response.Content.ReadFromJsonAsync<LanOperationResult>(cancellationToken: ct);
            if (parsed is not null)
            {
                return parsed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse LAN operation response as LanOperationResult.");
        }

        var message = await ReadErrorMessageAsync(response, ct);

        if (response.IsSuccessStatusCode)
        {
            return new LanOperationResult(true, "LAN access state updated.");
        }

        return new LanOperationResult(false, message ?? "LAN access operation failed. Check server logs for details.");
    }

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>(cancellationToken: ct);
            if (payload is not null)
            {
                if (!string.IsNullOrWhiteSpace(payload.Error))
                {
                    return payload.Error;
                }

                if (!string.IsNullOrWhiteSpace(payload.Detail))
                {
                    return payload.Detail;
                }

                if (!string.IsNullOrWhiteSpace(payload.Message))
                {
                    return payload.Message;
                }
            }
        }
        catch
        {
            // Fall back to plain text.
        }

        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
        catch
        {
            return null;
        }
    }

    private sealed record ApiErrorPayload(string? Error, string? Detail, string? Message);
}
