using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Handlers;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.FiscalYears;

public sealed class FiscalYearGovernanceHandlersTests
{
    [Fact]
    public async Task CloseHandler_PersistsAndReturnsYearStatus()
    {
        var governance = new Mock<IFiscalYearGovernanceRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        governance.Setup(x => x.CloseYearAsync(2025, "admin", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        governance.Setup(x => x.ListStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FiscalYearStatusDto>
            {
                new(2025, true, DateTime.UtcNow, "admin", null, null)
            });

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CloseFiscalYearHandler(governance.Object, uow.Object);
        var result = await handler.HandleAsync(new CloseFiscalYearRequest(2025, "admin"), CancellationToken.None);

        result.Year.Should().Be(2025);
        result.IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task ReopenHandler_PersistsAndReturnsYearStatus()
    {
        var governance = new Mock<IFiscalYearGovernanceRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        governance.Setup(x => x.ReopenYearAsync(2025, "admin", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        governance.Setup(x => x.ListStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FiscalYearStatusDto>
            {
                new(2025, false, DateTime.UtcNow.AddDays(-1), "admin", DateTime.UtcNow, "admin")
            });

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new ReopenFiscalYearHandler(governance.Object, uow.Object);
        var result = await handler.HandleAsync(new ReopenFiscalYearRequest(2025, "admin"), CancellationToken.None);

        result.Year.Should().Be(2025);
        result.IsClosed.Should().BeFalse();
    }
}
