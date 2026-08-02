using System.Diagnostics;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Installer;

public sealed class PublishMsiLayoutTests
{
    [Fact]
    public void PublishMsiLayout_CopiesHostingBundleIntoInstallerPrereqs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = Path.Combine(Path.GetTempPath(), $"ff-msi-layout-tests-{Guid.NewGuid():N}");
        var sourceDistDir = Path.Combine(root, "source");
        var layoutDir = Path.Combine(root, "layout");
        var hostingBundlePath = Path.Combine(root, "dotnet-hosting-9.0-win.exe");

        Directory.CreateDirectory(sourceDistDir);
        File.WriteAllText(Path.Combine(sourceDistDir, "FamilyFinances.Web.exe"), "dummy");
        File.WriteAllText(hostingBundlePath, "bundle");

        try
        {
            RunPublishMsiLayout(sourceDistDir, layoutDir, hostingBundlePath);

            File.Exists(Path.Combine(layoutDir, "installer-prereqs", "dotnet-hosting-9.0-win.exe"))
                .Should().BeTrue();
            File.Exists(Path.Combine(layoutDir, "installer-scripts", "Invoke-HostingBundleMaintenance.ps1"))
                .Should().BeTrue();
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static void RunPublishMsiLayout(string sourceDistDir, string layoutDir, string hostingBundlePath)
    {
        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "tools", "installer", "windows", "scripts", "Publish-MsiLayout.ps1");

        scriptPath = Path.GetFullPath(scriptPath);

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments =
                $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" " +
                $"-Version \"test-version\" " +
                $"-SourceDistDir \"{sourceDistDir}\" " +
                $"-MsiLayoutDir \"{layoutDir}\" " +
                $"-HostingBundleSourcePath \"{hostingBundlePath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi);
        process.Should().NotBeNull();
        process!.WaitForExit();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        process.ExitCode.Should().Be(0, $"stdout: {stdout}{Environment.NewLine}stderr: {stderr}");
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Best effort cleanup for temp test folders.
        }
    }
}
