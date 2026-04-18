using FamilyFinances.Api.Features.HostOps;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace FamilyFinances.Api.IntegrationTests.HostOps;

public sealed class ScriptLanHostOperationsServiceTests
{
    [Fact]
    public async Task ApplyAsync_ReturnsFailure_WhenPortIsInvalid()
    {
        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetStatusScript());
        try
        {
            var sut = CreateSut(scriptsRoot);
            var result = await sut.ApplyAsync(
                new LanAccessRequest(true, LanAccessCommandValidator.ForbiddenApiPort, "host", false),
                actor: "admin",
                ct: CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Invalid HTTPS port");
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsParsedStatus_WhenScriptsReturnJson()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetStatusScript());
        try
        {
            var sut = CreateSut(scriptsRoot);
            var status = await sut.GetStatusAsync(CancellationToken.None);

            status.Enabled.Should().BeTrue();
            status.HttpsPort.Should().Be(5443);
            status.HostName.Should().Be("familyfinances.local");
            status.FirewallEnabled.Should().BeTrue();
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    [Fact]
    public async Task ApplyAsync_ReturnsSucceededResult_WhenSetAndGetScriptsSucceed()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetStatusScript());
        try
        {
            var sut = CreateSut(scriptsRoot);
            var result = await sut.ApplyAsync(
                new LanAccessRequest(true, 5443, "familyfinances.local", false),
                actor: "admin",
                ct: CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Status.Should().NotBeNull();
            result.Status!.HttpsPort.Should().Be(5443);
            result.Message.Should().Be("LAN access state updated.");
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    [Fact]
    public async Task ApplyAsync_ReturnsAdminPermissionMessage_WhenScriptFailsWithAccessDenied()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetFailureScript("UnauthorizedAccessException"));
        try
        {
            var sut = CreateSut(scriptsRoot);
            var result = await sut.ApplyAsync(
                new LanAccessRequest(true, 5443, "familyfinances.local", false),
                actor: "admin",
                ct: CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.Message.Should().Be("LAN access changes require administrator permissions on this machine.");
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    [Fact]
    public async Task ApplyAsync_ReturnsDetailedFailureMessage_WhenScriptFailsWithoutAdminHint()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetFailureScript("Unexpected script failure"));
        try
        {
            var sut = CreateSut(scriptsRoot);
            var result = await sut.ApplyAsync(
                new LanAccessRequest(true, 5443, "familyfinances.local", false),
                actor: "admin",
                ct: CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("LAN access operation failed:");
            result.Message.Should().Contain("Unexpected");
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    [Fact]
    public async Task RegenerateCertificateAsync_UsesApplyPath_AndReturnsFailureForInvalidPort()
    {
        var scriptsRoot = CreateScriptsRoot(GetStatusScript(), SetStatusScript());
        try
        {
            var sut = CreateSut(scriptsRoot);
            var result = await sut.RegenerateCertificateAsync(
                LanAccessCommandValidator.ForbiddenApiPort,
                "familyfinances.local",
                "admin",
                CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.Message.Should().Contain("Invalid HTTPS port");
        }
        finally
        {
            TryDeleteDirectory(scriptsRoot);
        }
    }

    private static ScriptLanHostOperationsService CreateSut(string scriptsRoot)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HostOperations:ScriptsRoot"] = scriptsRoot
            })
            .Build();

        return new ScriptLanHostOperationsService(
            NullLogger<ScriptLanHostOperationsService>.Instance,
            configuration);
    }

    private static string CreateScriptsRoot(string getScriptContent, string setScriptContent)
    {
        var root = Path.Combine(Path.GetTempPath(), $"ff-api-hostops-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "Get-LanAccessStatus.ps1"), getScriptContent);
        File.WriteAllText(Path.Combine(root, "Set-LanAccess.ps1"), setScriptContent);
        return root;
    }

    private static string GetStatusScript()
    {
        return """
               param([switch]$AsJson)
               Write-Output '{"enabled":true,"httpsPort":5443,"hostName":"familyfinances.local","certificateThumb":"THUMB","certificateSubject":"CN=familyfinances.local","firewallRuleName":"FamilyFinances.Web.LAN.HTTPS","firewallEnabled":true}'
               """;
    }

    private static string SetStatusScript()
    {
        return """
               param(
                 [string]$Enabled,
                 [int]$HttpsPort,
                 [string]$HostName,
                 [string]$RegenerateCertificate,
                 [switch]$AsJson
               )
               Write-Output '{"result":"ok"}'
               """;
    }

    private static string SetFailureScript(string message)
    {
        return $"""
                Write-Error "{message}"
                exit 1
                """;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch
        {
            // Best effort cleanup for temp test folders.
        }
    }
}
