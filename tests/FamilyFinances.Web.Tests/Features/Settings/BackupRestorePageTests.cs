using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Web.Api;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace FamilyFinances.Web.Tests.Features.Settings;

public sealed class BackupRestorePageTests : TestContext
{
    [Fact]
    public void Render_ShowsBackupAndRestoreSections_ForAdmin()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        RegisterServices(CreateHttpClient((_, _) => new HttpResponseMessage(HttpStatusCode.OK)));

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<BackupRestorePage>();

        cut.Markup.Should().Contain("Backup &amp; Restore");
        cut.Markup.Should().Contain("Safety Notice");
        cut.Find("[data-testid='backup-restore-create-button']");
        cut.Find("[data-testid='backup-restore-file-input']");
        cut.Find("[data-testid='backup-restore-apply-button']");
    }

    [Fact]
    public void ApplyButton_RemainsDisabled_UntilRestoreConfirmationMatches()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        RegisterServices(CreateHttpClient((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("api/v1/backup/restore/precheck", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new RestorePrecheckResultDto(
                        IsCompatible: true,
                        FormatVersion: "1.0",
                        SourceAppVersion: "0.9.6",
                        CreatedAtUtc: DateTimeOffset.UtcNow,
                        SourceMigration: "migration",
                        Errors: [],
                        Warnings: []))
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }));

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<BackupRestorePage>();
        var applyButton = cut.Find("[data-testid='backup-restore-apply-button']");
        applyButton.HasAttribute("disabled").Should().BeTrue();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary(
                new byte[] { 1, 2, 3 },
                "sample.ffbackup",
                null,
                "application/octet-stream"));

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='backup-restore-precheck-summary']");
            cut.Find("[data-testid='backup-restore-apply-button']").HasAttribute("disabled").Should().BeTrue();
        });

        cut.Find("[data-testid='backup-restore-confirmation-input']").Change("RESTORE");

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='backup-restore-apply-button']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public void CreateBackup_ShowsSuccessMessage_AndCallsDownloadInterop()
    {
        var downloadInvocation = JSInterop
            .SetupVoid("familyFinancesCharts.downloadStreamFile", _ => true);
        downloadInvocation.SetVoidResult();
        JSInterop.Mode = JSRuntimeMode.Strict;

        RegisterServices(CreateHttpClient((request, _) =>
        {
            if (request.RequestUri!.ToString().Contains("api/v1/backup/export", StringComparison.OrdinalIgnoreCase))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent([1, 2, 3, 4])
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "\"familyfinances-backup-20260222-100000.ffbackup\""
                };
                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }));

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<BackupRestorePage>();
        cut.Find("[data-testid='backup-restore-create-button']").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='backup-restore-success']").TextContent.Should().Contain("Backup created successfully");
            downloadInvocation.Invocations.Count.Should().Be(1);
        });
    }

    [Fact]
    public void ApplyRestore_WithRequiresReauth_RedirectsToLogin()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        var clearSessionInvocation = JSInterop.SetupVoid("sessionHelper.clearSessionActive", _ => true);
        clearSessionInvocation.SetVoidResult();

        RegisterServices(CreateHttpClient((request, _) =>
        {
            var uri = request.RequestUri!.ToString();

            if (uri.Contains("api/v1/backup/restore/precheck", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new RestorePrecheckResultDto(
                        IsCompatible: true,
                        FormatVersion: "1.0",
                        SourceAppVersion: "0.9.6",
                        CreatedAtUtc: DateTimeOffset.UtcNow,
                        SourceMigration: "migration",
                        Errors: [],
                        Warnings: []))
                };
            }

            if (uri.Contains("api/v1/backup/restore/apply", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new RestoreApplyResultDto(
                        Applied: true,
                        AppliedAtUtc: DateTimeOffset.UtcNow,
                        RequiresReauthentication: true,
                        FormatVersion: "1.0",
                        SourceAppVersion: "0.9.6",
                        SourceMigration: "migration",
                        Errors: [],
                        Warnings: []))
                };
            }

            if (uri.Contains("/auth/session", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }));

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<BackupRestorePage>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary(
                new byte[] { 1, 2, 3 },
                "sample.ffbackup",
                null,
                "application/octet-stream"));

        cut.WaitForAssertion(() => cut.Find("[data-testid='backup-restore-precheck-summary']"));
        cut.Find("[data-testid='backup-restore-confirmation-input']").Change("RESTORE");
        cut.Find("[data-testid='backup-restore-apply-button']").Click();

        cut.WaitForAssertion(() =>
        {
            nav.Uri.Should().Contain("/login?reason=restore");
        });
    }

    private void RegisterServices(HttpClient httpClient)
    {
        var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var tokenStore = new TestTokenStore("test-token");
        Services.AddSingleton(factoryMock.Object);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(new JwtAuthStateProvider(tokenStore));
        Services.AddScoped<BackupApi>();
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responder)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) => responder(request, ct));

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public TestTokenStore(string? token)
        {
            _token = token;
        }

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
