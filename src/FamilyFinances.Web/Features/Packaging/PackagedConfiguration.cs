using Microsoft.Extensions.Configuration;

namespace FamilyFinances.Web.Features.Packaging;

public static class PackagedConfiguration
{
    public const string ConfigRootEnvironmentVariable = "FF_CONFIG_ROOT";
    public const string RuntimeRootEnvironmentVariable = "FF_RUNTIME_ROOT";

    public static string? ResolveConfigRoot(string? configuredRoot = null)
    {
        var candidate = configuredRoot;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariable);
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            var runtimeRoot = Environment.GetEnvironmentVariable(RuntimeRootEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(runtimeRoot))
            {
                candidate = Path.Combine(runtimeRoot, "config", "web");
            }
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return null;
        }

        return Path.GetFullPath(candidate);
    }

    public static void Apply(ConfigurationManager configuration, string environmentName, string? configuredRoot = null)
    {
        var isDevelopment = string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase);
        var hasExplicitConfigRoot = !string.IsNullOrWhiteSpace(configuredRoot) ||
                                    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConfigRootEnvironmentVariable));

        // When working from source, ignore machine-wide packaged runtime roots to avoid
        // mixing production-installed config with local development settings.
        if (isDevelopment && !hasExplicitConfigRoot)
        {
            return;
        }

        var configRoot = ResolveConfigRoot(configuredRoot);
        if (configRoot is null)
        {
            return;
        }

        if (!Directory.Exists(configRoot))
        {
            throw new DirectoryNotFoundException($"Packaged config root not found: {configRoot}");
        }

        var baseConfigPath = Path.Combine(configRoot, "appsettings.json");
        if (!File.Exists(baseConfigPath))
        {
            throw new FileNotFoundException($"Missing packaged configuration file: {baseConfigPath}");
        }

        var envConfigPath = Path.Combine(configRoot, $"appsettings.{environmentName}.json");

        // Add packaged JSON providers with higher precedence than default appsettings providers.
        configuration.AddJsonFile(baseConfigPath, optional: false, reloadOnChange: false);
        configuration.AddJsonFile(envConfigPath, optional: true, reloadOnChange: false);

        // Re-add environment variables so they remain the top-level override source.
        configuration.AddEnvironmentVariables();
    }
}
