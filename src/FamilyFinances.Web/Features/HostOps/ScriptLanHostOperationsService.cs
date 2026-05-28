using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace FamilyFinances.Web.Features.HostOps;

public sealed class ScriptLanHostOperationsService : ILanHostOperationsService
{
    private readonly ILogger<ScriptLanHostOperationsService> _logger;
    private readonly IConfiguration _configuration;

    public ScriptLanHostOperationsService(
        ILogger<ScriptLanHostOperationsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var output = await RunScriptAsync("Get-LanAccessStatus.ps1", ["-AsJson"], ct);
        var parsed = JsonSerializer.Deserialize<LanAccessStatus>(output, JsonOptions());
        if (parsed is null)
        {
            throw new InvalidOperationException("Could not parse LAN status output.");
        }

        return parsed;
    }

    public async Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
    {
        if (!LanAccessCommandValidator.IsValidPort(request.HttpsPort))
        {
            return new LanOperationResult(false, $"Invalid HTTPS port: {request.HttpsPort}.");
        }

        try
        {
            var host = LanAccessCommandValidator.NormalizeHostName(request.HostName);
            var enabledArg = request.Enabled ? "1" : "0";
            var regenerateArg = request.RegenerateCertificate ? "1" : "0";

            var output = await RunScriptAsync(
                "Set-LanAccess.ps1",
                [
                    "-Enabled", enabledArg,
                    "-HttpsPort", request.HttpsPort.ToString(CultureInfo.InvariantCulture),
                    "-HostName", host,
                    "-RegenerateCertificate", regenerateArg,
                    "-AsJson"
                ],
                ct);

            var status = JsonSerializer.Deserialize<LanAccessStatus>(output, JsonOptions());
            _logger.LogInformation(
                "LAN host operation applied at {TimestampUtc}. Enabled={Enabled}. Port={Port}.",
                DateTime.UtcNow,
                request.Enabled,
                request.HttpsPort);

            return new LanOperationResult(true, "LAN access state updated.", status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LAN host operation failed at {TimestampUtc}.", DateTime.UtcNow);
            return new LanOperationResult(false, BuildOperationFailureMessage(ex));
        }
    }

    public Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default)
    {
        return ApplyAsync(
            new LanAccessRequest(
                Enabled: true,
                HttpsPort: httpsPort,
                HostName: hostName,
                RegenerateCertificate: true),
            actor,
            ct);
    }

    private async Task<string> RunScriptAsync(string scriptName, IEnumerable<string> arguments, CancellationToken ct)
    {
        var scriptsRoot = ResolveScriptsRoot();
        var scriptPath = Path.Combine(scriptsRoot, scriptName);
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Host operation script not found: {scriptPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Script '{scriptName}' failed (exit {process.ExitCode}): {stderr}");
        }

        return stdout.Trim();
    }

    private string ResolveScriptsRoot()
    {
        var candidates = EnumerateScriptsRootCandidates()
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (HasRequiredScripts(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Host operation scripts root could not be resolved. Checked paths: "
            + string.Join("; ", candidates));
    }

    private IEnumerable<string> EnumerateScriptsRootCandidates()
    {
        var configured = _configuration["HostOperations:ScriptsRoot"];
        foreach (var candidate in ExpandPathCandidates(configured))
        {
            yield return candidate;
        }

        var envPath = Environment.GetEnvironmentVariable("FF_HOSTOPS_SCRIPTS_ROOT");
        foreach (var candidate in ExpandPathCandidates(envPath))
        {
            yield return candidate;
        }

        var runtimeRoot = Environment.GetEnvironmentVariable("FF_RUNTIME_ROOT");
        foreach (var rootCandidate in ExpandPathCandidates(runtimeRoot))
        {
            yield return Path.Combine(rootCandidate, "installer-scripts");
        }

        yield return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "installer-scripts"));

        foreach (var parent in EnumerateParentDirectories(AppContext.BaseDirectory))
        {
            yield return Path.Combine(parent, "tools", "installer", "windows", "scripts");
            yield return Path.Combine(parent, "installer-scripts");
        }
    }

    private static IEnumerable<string> ExpandPathCandidates(string? pathValue)
    {
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            yield break;
        }

        if (Path.IsPathRooted(pathValue))
        {
            yield return Path.GetFullPath(pathValue);
            yield break;
        }

        yield return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, pathValue));
        yield return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), pathValue));
    }

    private static IEnumerable<string> EnumerateParentDirectories(string startPath)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startPath));
        while (current is not null)
        {
            yield return current.FullName;
            current = current.Parent;
        }
    }

    private static bool HasRequiredScripts(string rootPath)
    {
        return Directory.Exists(rootPath)
               && File.Exists(Path.Combine(rootPath, "Get-LanAccessStatus.ps1"))
               && File.Exists(Path.Combine(rootPath, "Set-LanAccess.ps1"));
    }

    private static string BuildOperationFailureMessage(Exception ex)
    {
        if (ContainsAny(ex,
            "Administrator privileges are required",
            "debe tener un estado elevado",
            "Access is denied",
            "UnauthorizedAccessException"))
        {
            return "LAN access changes require administrator permissions on this machine.";
        }

        return "LAN access operation failed. Check server logs for details.";
    }

    private static bool ContainsAny(Exception ex, params string[] fragments)
    {
        var current = ex;
        while (current is not null)
        {
            var message = current.Message;
            foreach (var fragment in fragments)
            {
                if (message.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            current = current.InnerException;
        }

        return false;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
