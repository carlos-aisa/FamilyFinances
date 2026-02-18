using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class UpdateMultiSplitTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesTransaction_WhenYearIsOpen()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);
        var txId = Guid.NewGuid();
        var existing = CreateTransaction(txId, new DateOnly(2025, 1, 1));

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.UpdateMultiSplitAsync(
                txId,
                new DateOnly(2025, 1, 2),
                "Updated multi-split",
                null,
                It.IsAny<IReadOnlyList<TransactionSplitInput>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateMultiSplitTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);
        var request = new UpdateMultiSplitTransactionRequest(
            txId,
            new DateOnly(2025, 1, 2),
            "Updated multi-split",
            null,
            new List<TransactionSplitInput>
            {
                new(Guid.NewGuid(), -1000, "From"),
                new(Guid.NewGuid(), 500, "Part A"),
                new(Guid.NewGuid(), 500, "Part B")
            });

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.Should().BeTrue();
        fiscalYearGuard.Verify(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenYearIsClosed()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);
        var txId = Guid.NewGuid();
        var existing = CreateTransaction(txId, new DateOnly(2025, 1, 1));

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Year 2025 is closed. Reopen the year to modify movements."));

        var handler = new UpdateMultiSplitTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);
        var request = new UpdateMultiSplitTransactionRequest(
            txId,
            new DateOnly(2025, 1, 2),
            "Blocked multi-split",
            null,
            new List<TransactionSplitInput>
            {
                new(Guid.NewGuid(), -1000, null),
                new(Guid.NewGuid(), 1000, null)
            });

        var act = () => handler.HandleAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");

        repo.Verify(r => r.UpdateMultiSplitAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<IReadOnlyList<TransactionSplitInput>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Transaction CreateTransaction(Guid id, DateOnly bookedOn)
    {
        var from = AccountId.New();
        var to = AccountId.New();

        return Transaction.Create(
            bookedOn,
            "Existing",
            new[]
            {
                TransactionSplit.Create(from, new Money(-1000), "From"),
                TransactionSplit.Create(to, new Money(1000), "To")
            },
            null,
            id);
    }
}
