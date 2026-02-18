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

public sealed class UpdateTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesTransaction_ReturnsTrue()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var bookedOn = new DateOnly(2026, 1, 10);
        var existing = CreateTransaction(txId, new DateOnly(2026, 1, 5));

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.UpdateTwoSplitAsync(
                txId,
                bookedOn,
                "Updated Transfer",
                null,
                fromAccountId,
                toAccountId,
                75.50m,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);

        var request = new UpdateTransactionRequest(
            txId,
            bookedOn,
            "Updated Transfer",
            null,
            fromAccountId,
            toAccountId,
            75.50m);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.Should().BeTrue();
        repo.Verify(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()), Times.Once);
        fiscalYearGuard.Verify(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()), Times.Exactly(2));
        repo.Verify(r => r.UpdateTwoSplitAsync(
            txId,
            bookedOn,
            "Updated Transfer",
            null,
            fromAccountId,
            toAccountId,
            75.50m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ReturnsFalse()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);
        var txId = Guid.NewGuid();

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);

        var request = new UpdateTransactionRequest(
            txId,
            new DateOnly(2026, 1, 10),
            "Test",
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            50.00m);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.Should().BeFalse();
        repo.Verify(r => r.UpdateTwoSplitAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithPayee_ValidatesExistingAndTargetYears()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var payeeId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var existing = CreateTransaction(txId, new DateOnly(2025, 12, 31));

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.UpdateTwoSplitAsync(
                txId,
                new DateOnly(2026, 1, 15),
                "Payment to vendor",
                payeeId,
                fromAccountId,
                toAccountId,
                100.00m,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);

        var request = new UpdateTransactionRequest(
            txId,
            new DateOnly(2026, 1, 15),
            "Payment to vendor",
            payeeId,
            fromAccountId,
            toAccountId,
            100.00m);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.Should().BeTrue();
        fiscalYearGuard.Verify(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()), Times.Once);
        fiscalYearGuard.Verify(x => x.EnsureYearOpenAsync(2026, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenFiscalYearIsClosed()
    {
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var fiscalYearGuard = new Mock<IFiscalYearGuard>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var existing = CreateTransaction(txId, new DateOnly(2025, 10, 1));

        repo.Setup(r => r.GetByIdAsync(new TransactionId(txId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        fiscalYearGuard
            .Setup(x => x.EnsureYearOpenAsync(2025, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Year 2025 is closed. Reopen the year to modify movements."));

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object, fiscalYearGuard.Object);
        var request = new UpdateTransactionRequest(
            txId,
            new DateOnly(2025, 10, 2),
            "Blocked update",
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            10m);

        var act = () => handler.HandleAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Message.Should().Contain("Year 2025 is closed");

        repo.Verify(r => r.UpdateTwoSplitAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
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
