using FamilyFinances.Api.Features.HostOps;
using FluentAssertions;

namespace FamilyFinances.Api.IntegrationTests.HostOps;

public sealed class LanAccessCommandValidatorTests
{
    [Theory]
    [InlineData(1024)]
    [InlineData(5084)]
    [InlineData(65536)]
    public void IsValidPort_ReturnsFalse_ForOutOfRangeOrForbiddenPort(int port)
    {
        LanAccessCommandValidator.IsValidPort(port).Should().BeFalse();
    }

    [Fact]
    public void IsValidPort_ReturnsTrue_ForValidPort()
    {
        LanAccessCommandValidator.IsValidPort(5443).Should().BeTrue();
    }

    [Fact]
    public void NormalizeHostName_ReturnsMachineName_WhenInputIsBlank()
    {
        LanAccessCommandValidator.NormalizeHostName("   ")
            .Should()
            .Be(Environment.MachineName);
    }

    [Fact]
    public void NormalizeHostName_ReturnsTrimmedHost_WhenInputIsProvided()
    {
        LanAccessCommandValidator.NormalizeHostName("  familyfinances.local  ")
            .Should()
            .Be("familyfinances.local");
    }

    [Theory]
    [InlineData("familyfinances.local")]
    [InlineData("192.168.1.10")]
    [InlineData("localhost")]
    public void IsSafeHostName_ReturnsTrue_ForValidHosts(string host)
    {
        LanAccessCommandValidator.IsSafeHostName(host).Should().BeTrue();
    }

    [Theory]
    [InlineData("host;whoami")]
    [InlineData("host | whoami")]
    [InlineData("host && whoami")]
    [InlineData("$env:Path")]
    [InlineData("\"")]
    public void IsSafeHostName_ReturnsFalse_ForUnsafeHosts(string host)
    {
        LanAccessCommandValidator.IsSafeHostName(host).Should().BeFalse();
    }

    [Fact]
    public void NormalizeHostName_ThrowsForUnsafeHost()
    {
        var act = () => LanAccessCommandValidator.NormalizeHostName("host;whoami");

        act.Should().Throw<ArgumentException>();
    }
}
