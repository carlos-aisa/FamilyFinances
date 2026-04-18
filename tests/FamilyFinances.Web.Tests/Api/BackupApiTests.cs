using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Api;

public sealed class BackupApiTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IApiTokenStore> _tokenStoreMock = new();
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly BackupApi _sut;

    public BackupApiTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient("FamilyFinancesApi"))
            .Returns(httpClient);

        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns("valid-token");

        _tokenStoreMock
            .Setup(store => store.WaitForTokenAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("valid-token");

        _sut = new BackupApi(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ExportBackupAsync_ReturnsFile_UsingContentDispositionFileName()
    {
        var content = new ByteArrayContent([1, 2, 3, 4]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = "familyfinances-test.ffbackup"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Get && request.RequestUri!.ToString().Contains("/backup/export", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });

        var result = await _sut.ExportBackupAsync(CancellationToken.None);

        result.FileName.Should().Be("familyfinances-test.ffbackup");
        result.ContentType.Should().Be("application/zip");
        result.Content.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public async Task ExportBackupAsync_UsesFallbackFileName_WhenHeaderDoesNotContainFileName()
    {
        var content = new ByteArrayContent([9, 8, 7]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });

        var result = await _sut.ExportBackupAsync(CancellationToken.None);

        result.FileName.Should().StartWith("familyfinances-backup-");
        result.FileName.Should().EndWith(".ffbackup");
    }

    [Fact]
    public async Task ExportBackupAsync_ThrowsUnauthorizedAccessException_WhenTokenIsMissing()
    {
        _tokenStoreMock
            .Setup(store => store.GetAccessToken())
            .Returns(string.Empty);
        _tokenStoreMock
            .Setup(store => store.WaitForTokenAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var act = () => _sut.ExportBackupAsync(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No access token available.");
    }

    [Fact]
    public async Task GetDatabaseInfoAsync_ReturnsNull_WhenJsonCannotBeParsed()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/database-info", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json")
            });

        var result = await _sut.GetDatabaseInfoAsync(CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDatabaseInfoAsync_ThrowsBackupApiException_OnApiError()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/database-info", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(new BackupApiErrorDto("Database path unavailable.", "Unavailable"))
            });

        var act = () => _sut.GetDatabaseInfoAsync(CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BackupApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Reason.Should().Be("Unavailable");
        exception.Which.Message.Should().Be("Database path unavailable.");
    }

    [Fact]
    public async Task PrecheckRestoreAsync_SendsMultipartPayload_AndReturnsDto()
    {
        HttpRequestMessage? captured = null;
        var payload = new RestorePrecheckResultDto(
            IsCompatible: true,
            FormatVersion: "1.0",
            SourceAppVersion: "1.1.2",
            CreatedAtUtc: DateTimeOffset.UtcNow,
            SourceMigration: "202604180001",
            Errors: [],
            Warnings: []);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            });

        var result = await _sut.PrecheckRestoreAsync(new StubBrowserFile("backup.ffbackup", [1, 2, 3]), CancellationToken.None);

        result.Should().BeEquivalentTo(payload);
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Post);
        captured.RequestUri!.ToString().Should().Contain("/backup/restore/precheck");
        captured.Content.Should().BeOfType<MultipartFormDataContent>();
    }

    [Fact]
    public async Task ApplyRestoreAsync_ThrowsBackupApiException_WhenApiReturnsErrorPayload()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.RequestUri!.ToString().Contains("/backup/restore/apply", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(new BackupApiErrorDto("Incompatible package.", "IncompatiblePackage"))
            });

        var act = () => _sut.ApplyRestoreAsync(new StubBrowserFile("backup.ffbackup", [1, 2, 3]), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BackupApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.Which.Reason.Should().Be("IncompatiblePackage");
        exception.Which.Message.Should().Be("Incompatible package.");
    }

    private sealed class StubBrowserFile : IBrowserFile
    {
        private readonly byte[] _content;

        public StubBrowserFile(string name, byte[] content)
        {
            Name = name;
            _content = content;
        }

        public string Name { get; }
        public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;
        public long Size => _content.LongLength;
        public string ContentType { get; } = "application/octet-stream";

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return new MemoryStream(_content);
        }
    }
}
