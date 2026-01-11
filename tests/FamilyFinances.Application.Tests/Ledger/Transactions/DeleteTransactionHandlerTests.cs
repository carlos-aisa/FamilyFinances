using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class DeleteTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTransactionExists_DeletesTransactionAndReturnsTrue()
    {
        // Arrange
        var bank = AccountId.New();
        var expense = AccountId.New();
        
        var tx = Transaction.Create(
            new DateOnly(2026, 1, 2),
            "Groceries",
            new[]
            {
                TransactionSplit.Create(bank, new Money(-5000), "Payment"),
                TransactionSplit.Create(expense, new Money(5000), "Expense")
            });

        var ct = CancellationToken.None;
        
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(tx.Id, ct))
            .ReturnsAsync(tx);

        repo.Setup(r => r.RemoveAsync(tx.Id, ct))
            .Returns(Task.CompletedTask);

        uow.Setup(u => u.SaveChangesAsync(ct))
            .ReturnsAsync(1);

        var handler = new DeleteTransactionHandler(uow.Object, repo.Object);

        // Act
        var result = await handler.HandleAsync(tx.Id.Value, ct);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdAsync(tx.Id, ct), Times.Once);
        repo.Verify(r => r.RemoveAsync(tx.Id, ct), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionDoesNotExist_ReturnsFalseWithoutDeleting()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var txId = new TransactionId(transactionId);
        var ct = CancellationToken.None;

        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(txId, ct))
            .ReturnsAsync((Transaction?)null);

        var handler = new DeleteTransactionHandler(uow.Object, repo.Object);

        // Act
        var result = await handler.HandleAsync(transactionId, ct);

        // Assert
        result.Should().BeFalse();

        repo.Verify(r => r.GetByIdAsync(txId, ct), Times.Once);
        repo.Verify(r => r.RemoveAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ConvertsGuidToTransactionId()
    {
        // Arrange
        var bank = AccountId.New();
        var expense = AccountId.New();
        
        var tx = Transaction.Create(
            new DateOnly(2026, 1, 2),
            "Groceries",
            new[]
            {
                TransactionSplit.Create(bank, new Money(-5000), "Payment"),
                TransactionSplit.Create(expense, new Money(5000), "Expense")
            });

        var expectedTxId = tx.Id;
        var ct = CancellationToken.None;
        
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        TransactionId? capturedId = null;

        repo.Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), ct))
            .ReturnsAsync(tx)
            .Callback<TransactionId, CancellationToken>((id, _) => capturedId = id);

        repo.Setup(r => r.RemoveAsync(It.IsAny<TransactionId>(), ct))
            .Returns(Task.CompletedTask);

        uow.Setup(u => u.SaveChangesAsync(ct))
            .ReturnsAsync(1);

        var handler = new DeleteTransactionHandler(uow.Object, repo.Object);

        // Act
        await handler.HandleAsync(tx.Id.Value, ct);

        // Assert
        capturedId.Should().NotBeNull();
        capturedId.Should().Be(expectedTxId);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToAllDependencies()
    {
        // Arrange
        var bank = AccountId.New();
        var expense = AccountId.New();
        
        var tx = Transaction.Create(
            new DateOnly(2026, 1, 2),
            "Groceries",
            new[]
            {
                TransactionSplit.Create(bank, new Money(-5000), "Payment"),
                TransactionSplit.Create(expense, new Money(5000), "Expense")
            });

        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(tx.Id, ct))
            .ReturnsAsync(tx);

        repo.Setup(r => r.RemoveAsync(tx.Id, ct))
            .Returns(Task.CompletedTask);

        uow.Setup(u => u.SaveChangesAsync(ct))
            .ReturnsAsync(1);

        var handler = new DeleteTransactionHandler(uow.Object, repo.Object);

        // Act
        await handler.HandleAsync(tx.Id.Value, ct);

        // Assert
        repo.Verify(r => r.GetByIdAsync(tx.Id, ct), Times.Once);
        repo.Verify(r => r.RemoveAsync(tx.Id, ct), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
