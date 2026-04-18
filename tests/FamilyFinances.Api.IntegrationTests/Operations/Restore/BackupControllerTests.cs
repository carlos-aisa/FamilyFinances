using System.Text.Json;
using FamilyFinances.Api.Controllers.V1;
using FamilyFinances.Application.Operations.BackupRestore.Abstractions;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Application.Operations.BackupRestore.Handlers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace FamilyFinances.Api.IntegrationTests.Operations.Restore;

public sealed class BackupControllerTests
{
    [Fact]
    public async Task Export_ReturnsFileContentResult_WhenBackupIsCreated()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService
        {
            CreateBackup = _ => Task.FromResult(
                new BackupArtifactDto(
                    "familyfinances.ffbackup",
                    "application/octet-stream",
                    [1, 2, 3],
                    new BackupPackageManifestDto(
                        "1.0",
                        "1.1.2",
                        DateTimeOffset.UtcNow,
                        "202604180001",
                        "checksum",
                        ["Accounts"])))
        };
        var handler = new CreateBackupHandler(service, new StubBackupOperationLock(new NoopAsyncDisposable()));

        var result = await sut.Export(handler, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.FileDownloadName.Should().Be("familyfinances.ffbackup");
        file.ContentType.Should().Be("application/octet-stream");
        file.FileContents.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Export_ReturnsConflict_WhenOperationIsAlreadyInProgress()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService();
        var handler = new CreateBackupHandler(service, new StubBackupOperationLock(null));

        var result = await sut.Export(handler, CancellationToken.None);

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        ReadReason(conflict.Value).Should().Be("OperationInProgress");
    }

    [Fact]
    public async Task Precheck_ReturnsConflict_WhenOperationIsAlreadyInProgress()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService();
        var handler = new PrecheckRestoreHandler(service, new StubBackupOperationLock(null));
        var file = CreateFormFile([1, 2, 3], "backup.ffbackup");

        var result = await sut.Precheck(file, handler, CancellationToken.None);

        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        ReadReason(conflict.Value).Should().Be("OperationInProgress");
    }

    [Fact]
    public async Task Apply_ReturnsBadRequest_WhenRestoreApplyFails()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService
        {
            Precheck = (_, _) => Task.FromResult(
                new RestorePrecheckResultDto(
                    true,
                    "1.0",
                    "1.1.2",
                    DateTimeOffset.UtcNow,
                    "202604180001",
                    [],
                    [])),
            Apply = (_, _) => Task.FromResult(
                new RestoreApplyResultDto(
                    false,
                    null,
                    false,
                    "1.0",
                    "1.1.2",
                    "202604180001",
                    [],
                    []))
        };

        var handler = new ApplyRestoreHandler(service, new StubBackupOperationLock(new NoopAsyncDisposable()));
        var file = CreateFormFile([1, 2, 3], "backup.ffbackup");

        var result = await sut.Apply(file, handler, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ReadReason(badRequest.Value).Should().Be("RestoreApplyFailed");
    }

    [Fact]
    public async Task Apply_ReturnsConflict_WhenOperationIsAlreadyInProgress()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService();
        var handler = new ApplyRestoreHandler(service, new StubBackupOperationLock(null));
        var file = CreateFormFile([1, 2, 3], "backup.ffbackup");

        var result = await sut.Apply(file, handler, CancellationToken.None);

        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        ReadReason(conflict.Value).Should().Be("OperationInProgress");
    }

    [Fact]
    public async Task Precheck_ReturnsBadRequest_WhenFileIsTooLarge()
    {
        var sut = new BackupController();
        var service = new StubBackupRestoreService();
        var handler = new PrecheckRestoreHandler(service, new StubBackupOperationLock(new NoopAsyncDisposable()));
        var file = new OversizedFormFile("oversized.ffbackup", 209_715_201);

        var result = await sut.Precheck(file, handler, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ReadReason(badRequest.Value).Should().Be("FileTooLarge");
    }

    [Fact]
    public void GetDatabaseInfo_ReturnsNull_WhenConnectionStringMissing()
    {
        var sut = new BackupController();
        var config = BuildConfiguration(connectionString: null);
        var environment = new StubHostEnvironment(Path.GetTempPath());

        var result = sut.GetDatabaseInfo(config, environment);

        var payload = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<BackupDatabaseInfoDto>().Subject;
        payload.DatabaseFilePath.Should().BeNull();
    }

    [Fact]
    public void GetDatabaseInfo_ReturnsMemoryDataSource_WhenConnectionStringUsesInMemory()
    {
        var sut = new BackupController();
        var config = BuildConfiguration("Data Source=:memory:");
        var environment = new StubHostEnvironment(Path.GetTempPath());

        var result = sut.GetDatabaseInfo(config, environment);

        var payload = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<BackupDatabaseInfoDto>().Subject;
        payload.DatabaseFilePath.Should().Be(":memory:");
    }

    [Fact]
    public void GetDatabaseInfo_ResolvesRelativePath_FromContentRoot()
    {
        var sut = new BackupController();
        var contentRoot = Path.Combine(Path.GetTempPath(), $"ff-backup-info-{Guid.NewGuid():N}");
        var config = BuildConfiguration("Data Source=data/familyfinances.db");
        var environment = new StubHostEnvironment(contentRoot);

        var result = sut.GetDatabaseInfo(config, environment);

        var payload = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<BackupDatabaseInfoDto>().Subject;
        payload.DatabaseFilePath.Should().Be(Path.GetFullPath(Path.Combine(contentRoot, "data/familyfinances.db")));
    }

    [Fact]
    public void GetDatabaseInfo_ReturnsNull_WhenConnectionStringIsInvalid()
    {
        var sut = new BackupController();
        var config = BuildConfiguration("not a valid connection string");
        var environment = new StubHostEnvironment(Path.GetTempPath());

        var result = sut.GetDatabaseInfo(config, environment);

        var payload = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<BackupDatabaseInfoDto>().Subject;
        payload.DatabaseFilePath.Should().BeNull();
    }

    private static IConfiguration BuildConfiguration(string? connectionString)
    {
        var values = new Dictionary<string, string?>();
        if (connectionString is not null)
            values["ConnectionStrings:Default"] = connectionString;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }

    private static string? ReadReason(object? value)
    {
        if (value is null)
            return null;

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value));
        return json.RootElement.TryGetProperty("reason", out var reason)
            ? reason.GetString()
            : null;
    }

    private sealed class StubBackupRestoreService : IBackupRestoreService
    {
        public Func<CancellationToken, Task<BackupArtifactDto>> CreateBackup { get; set; } = _ => throw new NotImplementedException();
        public Func<Stream, CancellationToken, Task<RestorePrecheckResultDto>> Precheck { get; set; } = (_, _) => throw new NotImplementedException();
        public Func<Stream, CancellationToken, Task<RestoreApplyResultDto>> Apply { get; set; } = (_, _) => throw new NotImplementedException();

        public Task<BackupArtifactDto> CreateBackupAsync(CancellationToken ct) => CreateBackup(ct);

        public Task<RestorePrecheckResultDto> PrecheckRestoreAsync(Stream packageStream, CancellationToken ct) => Precheck(packageStream, ct);

        public Task<RestoreApplyResultDto> ApplyRestoreAsync(Stream packageStream, CancellationToken ct) => Apply(packageStream, ct);
    }

    private sealed class StubBackupOperationLock : IBackupOperationLock
    {
        private readonly IAsyncDisposable? _handle;

        public StubBackupOperationLock(IAsyncDisposable? handle)
        {
            _handle = handle;
        }

        public Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken ct) => Task.FromResult(_handle);
    }

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class OversizedFormFile : IFormFile
    {
        public OversizedFormFile(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
        }

        public string ContentType { get; } = "application/octet-stream";
        public string ContentDisposition { get; } = "form-data";
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public long Length { get; }
        public string Name { get; } = "file";
        public string FileName { get; }

        public void CopyTo(Stream target)
        {
            using var stream = OpenReadStream();
            stream.CopyTo(target);
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            using var stream = OpenReadStream();
            stream.CopyTo(target);
            return Task.CompletedTask;
        }

        public Stream OpenReadStream() => new MemoryStream([1, 2, 3]);
    }

    private sealed class StubHostEnvironment : IWebHostEnvironment
    {
        public StubHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string ApplicationName { get; set; } = "FamilyFinances.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
