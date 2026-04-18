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
}
