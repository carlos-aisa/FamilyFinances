using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class ReconcileAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_WhenFiscalYearIsClosed()
    {
        var account = Account.Create(
            "Main Bank",
            AccountNature.Asset,
            AccountKind.Checking,
            new DateOnly(2024, 1, 1));

        var accounts = new Mock<IAccountRepository>(MockBehavior.Strict);
        var transactions = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var balanceService = new Mock<IAccountBalanceService>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        accounts.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Year 2025 is closed. Reopen the year to modify movements."));

        var handler = new ReconcileAccountHandler(
            accounts.Object,
            transactions.Object,
            balanceService.Object,
            fiscalYearGuard.Object,
            uow.Object);

        var request = new ReconcileAccountRequest(
            ActualBalance: 50m,
            AsOfDate: new DateOnly(2025, 12, 31),
            Note: null);

        var act = () => handler.HandleAsync(account.Id.Value, request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");

        transactions.Verify(x => x.AddAsync(It.IsAny<FamilyFinances.Domain.Ledger.Transactions.Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
