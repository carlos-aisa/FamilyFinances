using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.FiscalYears.Services;
using FamilyFinances.Domain.Common;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.FiscalYears;

public sealed class FiscalYearGuardTests
{
    [Fact]
    public async Task EnsureYearOpenAsync_Throws_WhenYearIsClosed()
    {
        var governance = new Mock<IFiscalYearGovernanceRepository>(MockBehavior.Strict);
        governance.Setup(x => x.IsYearClosedAsync(2025, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var guard = new FiscalYearGuard(governance.Object);

        var act = () => guard.EnsureYearOpenAsync(2025, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");
    }

    [Fact]
    public async Task EnsureYearOpenAsync_DoesNotThrow_WhenYearIsOpen()
    {
        var governance = new Mock<IFiscalYearGovernanceRepository>(MockBehavior.Strict);
        governance.Setup(x => x.IsYearClosedAsync(2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var guard = new FiscalYearGuard(governance.Object);

        await guard.EnsureYearOpenAsync(2026, CancellationToken.None);
    }

    [Fact]
    public async Task IsYearClosedAsync_DelegatesToRepository()
    {
        var governance = new Mock<IFiscalYearGovernanceRepository>(MockBehavior.Strict);
        governance.Setup(x => x.IsYearClosedAsync(2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var guard = new FiscalYearGuard(governance.Object);
        var result = await guard.IsYearClosedAsync(2024, CancellationToken.None);

        result.Should().BeTrue();
        governance.Verify(x => x.IsYearClosedAsync(2024, It.IsAny<CancellationToken>()), Times.Once);
    }
}
