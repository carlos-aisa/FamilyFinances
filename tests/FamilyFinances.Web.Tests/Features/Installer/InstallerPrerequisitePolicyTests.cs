using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Installer;

public sealed class InstallerPrerequisitePolicyTests
{
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    public void IsRebootPending_ReturnsExpectedValue(
        bool componentBasedServicingPending,
        bool windowsUpdatePending,
        bool pendingFileRenameOperations,
        bool expected)
    {
        var result = InstallerPrerequisitePolicy.IsRebootPending(
            componentBasedServicingPending,
            windowsUpdatePending,
            pendingFileRenameOperations);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("No", false)]
    [InlineData("False", false)]
    [InlineData("0", false)]
    [InlineData("Yes", true)]
    [InlineData("True", true)]
    [InlineData("Possible", true)]
    public void IsRestartNeeded_MatchesExpectedValues(string? value, bool expected)
    {
        InstallerPrerequisitePolicy.IsRestartNeeded(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void ShouldAttemptHostingBundleMaintenance_RespectsModuleAndRebootState(
        bool aspNetCoreModuleInstalled,
        bool rebootPending,
        bool expected)
    {
        var result = InstallerPrerequisitePolicy.ShouldAttemptHostingBundleMaintenance(
            aspNetCoreModuleInstalled,
            rebootPending);

        result.Should().Be(expected);
    }

    [Fact]
    public void BuildRebootRequiredMessage_IncludesSources_WhenProvided()
    {
        var message = InstallerPrerequisitePolicy.BuildRebootRequiredMessage(
            ["ComponentBasedServicing", "WindowsUpdate"]);

        message.Should().Contain("Windows restart is required");
        message.Should().Contain("ComponentBasedServicing");
        message.Should().Contain("WindowsUpdate");
    }
}
