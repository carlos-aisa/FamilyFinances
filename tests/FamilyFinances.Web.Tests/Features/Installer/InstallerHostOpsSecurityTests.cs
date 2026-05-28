using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Installer;

public sealed class InstallerHostOpsSecurityTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("short", true)]
    [InlineData("PRODUCTION_SECRET_KEY_CHANGE_THIS_IN_REAL_DEPLOYMENT_MIN_64_CHARS_0123456789ABCDEF", true)]
    [InlineData("this-is-a-custom-secret-with-minimum-32-characters", false)]
    public void InstallerSecretPolicy_RequiresRotation_MatchesSecurityRules(string? key, bool expected)
    {
        var result = InstallerSecretPolicy.RequiresRotation(key);
        result.Should().Be(expected);
    }

    [Fact]
    public void InstallerSecretPolicy_GenerateSecureJwtKey_ReturnsStrongValue()
    {
        var key1 = InstallerSecretPolicy.GenerateSecureJwtKey();
        var key2 = InstallerSecretPolicy.GenerateSecureJwtKey();

        key1.Should().NotBeNullOrWhiteSpace();
        key1.Length.Should().BeGreaterThan(InstallerSecretPolicy.MinimumJwtLength);
        key2.Should().NotBe(key1);
    }

    [Theory]
    [InlineData(5443, true)]
    [InlineData(1024, false)]
    [InlineData(1025, true)]
    [InlineData(65535, true)]
    [InlineData(65536, false)]
    [InlineData(5084, false)]
    public void LanAccessCommandValidator_IsValidPort_EnforcesBoundsAndApiGuard(int port, bool expected)
    {
        LanAccessCommandValidator.IsValidPort(port).Should().Be(expected);
    }

    [Fact]
    public void LanAccessCommandValidator_NormalizeHostName_UsesMachineNameWhenMissing()
    {
        var normalized = LanAccessCommandValidator.NormalizeHostName(" ");
        normalized.Should().Be(Environment.MachineName);
    }

    [Theory]
    [InlineData("familyfinances.local")]
    [InlineData("192.168.1.10")]
    [InlineData("localhost")]
    public void LanAccessCommandValidator_IsSafeHostName_ReturnsTrue_ForValidHosts(string host)
    {
        LanAccessCommandValidator.IsSafeHostName(host).Should().BeTrue();
    }

    [Theory]
    [InlineData("host;whoami")]
    [InlineData("host | whoami")]
    [InlineData("host && whoami")]
    [InlineData("$env:Path")]
    [InlineData("\"")]
    public void LanAccessCommandValidator_IsSafeHostName_ReturnsFalse_ForUnsafeHosts(string host)
    {
        LanAccessCommandValidator.IsSafeHostName(host).Should().BeFalse();
    }

    [Fact]
    public void LanAccessCommandValidator_NormalizeHostName_ThrowsForUnsafeHost()
    {
        var act = () => LanAccessCommandValidator.NormalizeHostName("host;whoami");

        act.Should().Throw<ArgumentException>();
    }
}
