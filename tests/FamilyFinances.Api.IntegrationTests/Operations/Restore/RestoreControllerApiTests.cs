using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.Operations.Restore;

public sealed class RestoreControllerApiTests
{
    [Fact]
    public async Task GetDatabaseInfo_ReturnsCurrentDatabaseFilePath()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out var dbPath);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        var response = await client.GetAsync("/api/v1/backup/database-info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<BackupDatabaseInfoResponse>();
        payload.Should().NotBeNull();
        payload!.DatabaseFilePath.Should().NotBeNullOrWhiteSpace();
        Path.GetFullPath(payload.DatabaseFilePath!).Should().Be(Path.GetFullPath(dbPath));
    }

    [Fact]
    public async Task Precheck_WithoutFile_ReturnsMissingFileValidationError()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        using var content = new MultipartFormDataContent();
        var response = await client.PostAsync("/api/v1/backup/restore/precheck", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Precheck_WithEmptyFile_ReturnsEmptyFileValidationError()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "empty.ffbackup");

        var response = await client.PostAsync("/api/v1/backup/restore/precheck", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var reason = await ReadReasonAsync(response);
        reason.Should().Be("EmptyFile");
    }

    [Fact]
    public async Task Precheck_WithInvalidExtension_ReturnsInvalidFileTypeValidationError()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([1, 2, 3]), "file", "backup.zip");

        var response = await client.PostAsync("/api/v1/backup/restore/precheck", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var reason = await ReadReasonAsync(response);
        reason.Should().Be("InvalidFileType");
    }

    [Fact]
    public async Task Apply_WithInvalidBackupPackage_ReturnsIncompatiblePackage()
    {
        using var factory = TestClient.CreateFactoryWithFreshDb(out _);
        using var client = await TestClient.CreateAuthorizedClientAsync(factory);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([1, 2, 3, 4]), "file", "invalid.ffbackup");

        var response = await client.PostAsync("/api/v1/backup/restore/apply", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var reason = await ReadReasonAsync(response);
        reason.Should().Be("IncompatiblePackage");
    }

    private static async Task<string?> ReadReasonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);
        return json.RootElement.TryGetProperty("reason", out var reason)
            ? reason.GetString()
            : null;
    }

    private sealed record BackupDatabaseInfoResponse(string? DatabaseFilePath);
}
