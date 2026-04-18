using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Web.Auth;
using Microsoft.AspNetCore.Components.Forms;

namespace FamilyFinances.Web.Api;

public sealed class BackupApi
{
    private const long MaxUploadSizeBytes = 209_715_200; // 200 MB
    private static readonly JsonSerializerOptions JsonReadOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IApiTokenStore _tokenStore;

    public BackupApi(IHttpClientFactory factory, IApiTokenStore tokenStore)
    {
        _http = factory.CreateClient("FamilyFinancesApi");
        _tokenStore = tokenStore;
    }

    public async Task<DownloadedBackupFileDto> ExportBackupAsync(CancellationToken ct = default)
    {
        var token = await GetRequiredTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/backup/export");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        await EnsureSuccessStatusAsync(response, ct);

        var fileName = ResolveFileName(response) ?? BuildFallbackBackupFileName();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);

        return new DownloadedBackupFileDto(fileName, contentType, bytes);
    }

    public async Task<RestorePrecheckResultDto> PrecheckRestoreAsync(
        IBrowserFile file,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var token = await GetRequiredTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/backup/restore/precheck");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await using var fileStream = file.OpenReadStream(MaxUploadSizeBytes, ct);
        using var form = BuildMultipartContent(file, fileStream);
        request.Content = form;

        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessStatusAsync(response, ct);

        var dto = await response.Content.ReadFromJsonAsync<RestorePrecheckResultDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Failed to deserialize restore pre-check response.");
    }

    public async Task<RestoreApplyResultDto> ApplyRestoreAsync(
        IBrowserFile file,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var token = await GetRequiredTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/backup/restore/apply");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await using var fileStream = file.OpenReadStream(MaxUploadSizeBytes, ct);
        using var form = BuildMultipartContent(file, fileStream);
        request.Content = form;

        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessStatusAsync(response, ct);

        var dto = await response.Content.ReadFromJsonAsync<RestoreApplyResultDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Failed to deserialize restore apply response.");
    }

    public async Task<BackupDatabaseInfoDto?> GetDatabaseInfoAsync(CancellationToken ct = default)
    {
        var token = await GetRequiredTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/backup/database-info");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessStatusAsync(response, ct);

        return await TryReadJsonAsync<BackupDatabaseInfoDto>(response, ct);
    }

    private async Task<string> GetRequiredTokenAsync(CancellationToken ct)
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            token = await _tokenStore.WaitForTokenAsync(TimeSpan.FromSeconds(3), ct);

        return string.IsNullOrWhiteSpace(token)
            ? throw new UnauthorizedAccessException("No access token available.")
            : token;
    }

    private static MultipartFormDataContent BuildMultipartContent(IBrowserFile file, Stream fileStream)
    {
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var form = new MultipartFormDataContent();
        form.Add(streamContent, "file", file.Name);
        return form;
    }

    private static string? ResolveFileName(HttpResponseMessage response)
    {
        var raw = response.Content.Headers.ContentDisposition?.FileNameStar
                  ?? response.Content.Headers.ContentDisposition?.FileName;

        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return raw.Trim('\"');
    }

    private static string BuildFallbackBackupFileName()
    {
        return $"familyfinances-backup-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.ffbackup";
    }

    private static async Task EnsureSuccessStatusAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("API call unauthorized. Missing or invalid token.");

        if (response.IsSuccessStatusCode)
            return;

        var error = await TryReadApiErrorAsync(response, ct);
        var message = error?.Error;
        if (string.IsNullOrWhiteSpace(message))
            message = $"Backup API call failed with status {(int)response.StatusCode}.";

        throw new BackupApiException(message, response.StatusCode, error?.Reason);
    }

    private static async Task<BackupApiErrorDto?> TryReadApiErrorAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<BackupApiErrorDto>(cancellationToken: ct);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<T?> TryReadJsonAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var content = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonReadOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

public sealed record DownloadedBackupFileDto(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed class BackupApiException : Exception
{
    public BackupApiException(string message, HttpStatusCode statusCode, string? reason)
        : base(message)
    {
        StatusCode = statusCode;
        Reason = reason;
    }

    public HttpStatusCode StatusCode { get; }
    public string? Reason { get; }
}

public sealed record BackupApiErrorDto(string? Error, string? Reason);

public sealed record BackupDatabaseInfoDto(string? DatabaseFilePath);
