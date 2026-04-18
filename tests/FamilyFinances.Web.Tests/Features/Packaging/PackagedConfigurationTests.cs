using System.IO;
using FamilyFinances.Web.Features.Packaging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FamilyFinances.Web.Tests.Features.Packaging;

public sealed class PackagedConfigurationTests
{
    [Fact]
    public void ResolveConfigRoot_ReturnsNull_WhenNoInputsProvided()
    {
        using var _ = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            [PackagedConfiguration.ConfigRootEnvironmentVariable] = null,
            [PackagedConfiguration.RuntimeRootEnvironmentVariable] = null
        });

        PackagedConfiguration.ResolveConfigRoot(string.Empty).Should().BeNull();
    }

    [Fact]
    public void ResolveConfigRoot_UsesRuntimeRoot_WhenConfiguredRootMissing()
    {
        var runtimeRoot = Path.Combine(Path.GetTempPath(), $"ff-web-runtime-{Guid.NewGuid():N}");

        using var _ = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            [PackagedConfiguration.ConfigRootEnvironmentVariable] = null,
            [PackagedConfiguration.RuntimeRootEnvironmentVariable] = runtimeRoot
        });

        var resolved = PackagedConfiguration.ResolveConfigRoot(string.Empty);

        resolved.Should().Be(Path.GetFullPath(Path.Combine(runtimeRoot, "config", "web")));
    }

    [Fact]
    public void Apply_Loads_Packaged_Appsettings_WhenConfigRootProvided()
    {
        var tempRoot = CreateTempConfigRoot();

        try
        {
            File.WriteAllText(Path.Combine(tempRoot, "appsettings.json"), "{\"Ui\":{\"Name\":\"base\"}}");
            File.WriteAllText(Path.Combine(tempRoot, "appsettings.Production.json"), "{\"Ui\":{\"Name\":\"production\"}}");

            var configuration = new ConfigurationManager();
            PackagedConfiguration.Apply(configuration, "Production", tempRoot);

            configuration["Ui:Name"].Should().Be("production");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void Apply_Throws_WhenBaseAppsettingsMissing()
    {
        var tempRoot = CreateTempConfigRoot();

        try
        {
            var configuration = new ConfigurationManager();
            var action = () => PackagedConfiguration.Apply(configuration, "Production", tempRoot);

            action.Should().Throw<FileNotFoundException>();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void Apply_DoesNotLoad_RuntimeRoot_Config_InDevelopment_WhenNoExplicitConfigRoot()
    {
        var runtimeRoot = Path.Combine(Path.GetTempPath(), $"ff-web-runtime-{Guid.NewGuid():N}");
        var packagedRoot = Path.Combine(runtimeRoot, "config", "web");
        Directory.CreateDirectory(packagedRoot);

        try
        {
            File.WriteAllText(Path.Combine(packagedRoot, "appsettings.json"), "{\"Ui\":{\"Name\":\"packaged\"}}");
            File.WriteAllText(Path.Combine(packagedRoot, "appsettings.Development.json"), "{\"Ui\":{\"Name\":\"packaged-development\"}}");

            var configuration = new ConfigurationManager();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ui:Name"] = "local-development"
            });

            using var _ = new EnvironmentVariableScope(new Dictionary<string, string?>
            {
                [PackagedConfiguration.ConfigRootEnvironmentVariable] = null,
                [PackagedConfiguration.RuntimeRootEnvironmentVariable] = runtimeRoot
            });

            PackagedConfiguration.Apply(configuration, "Development");

            configuration["Ui:Name"].Should().Be("local-development");
        }
        finally
        {
            if (Directory.Exists(runtimeRoot))
            {
                Directory.Delete(runtimeRoot, recursive: true);
            }
        }
    }

    private static string CreateTempConfigRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ff-web-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _previousValues = new(StringComparer.OrdinalIgnoreCase);

        public EnvironmentVariableScope(IReadOnlyDictionary<string, string?> changes)
        {
            foreach (var change in changes)
            {
                _previousValues[change.Key] = Environment.GetEnvironmentVariable(change.Key);
                Environment.SetEnvironmentVariable(change.Key, change.Value);
            }
        }

        public void Dispose()
        {
            foreach (var previous in _previousValues)
            {
                Environment.SetEnvironmentVariable(previous.Key, previous.Value);
            }
        }
    }
}
