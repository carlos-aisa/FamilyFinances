using System.IO;
using FamilyFinances.Web.Features.Packaging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FamilyFinances.Web.Tests.Features.Packaging;

public sealed class PackagedConfigurationTests
{
    [Fact]
    public void ResolveConfigRoot_ReturnsNull_WhenValueMissing()
    {
        PackagedConfiguration.ResolveConfigRoot(string.Empty).Should().BeNull();
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

    private static string CreateTempConfigRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ff-web-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }
}
