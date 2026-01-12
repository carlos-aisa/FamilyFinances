using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Handlers;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Transactions;

public sealed class UpdateTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesTransaction_ReturnsTrue()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var bookedOn = new DateOnly(2026, 1, 10);
        var description = "Updated Transfer";
        var amount = 75.50m;

        repo.Setup(r => r.UpdateTwoSplitAsync(
                txId,
                bookedOn,
                description,
                null,
                fromAccountId,
                toAccountId,
                amount,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object);

        var request = new UpdateTransactionRequest(
            Id: txId,
            BookedOn: bookedOn,
            Description: description,
            PayeeId: null,
            FromAccountId: fromAccountId,
            ToAccountId: toAccountId,
            Amount: amount);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.UpdateTwoSplitAsync(
            txId,
            bookedOn,
            description,
            null,
            fromAccountId,
            toAccountId,
            amount,
            It.IsAny<CancellationToken>()), Times.Once);

        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ReturnsFalse()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();

        repo.Setup(r => r.UpdateTwoSplitAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object);

        var request = new UpdateTransactionRequest(
            Id: txId,
            BookedOn: new DateOnly(2026, 1, 10),
            Description: "Test",
            PayeeId: null,
            FromAccountId: fromAccountId,
            ToAccountId: toAccountId,
            Amount: 50.00m);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        repo.Verify(r => r.UpdateTwoSplitAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithPayee_UpdatesTransactionWithPayeeId()
    {
        // Arrange
        var repo = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var txId = Guid.NewGuid();
        var payeeId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        var toAccountId = Guid.NewGuid();
        var bookedOn = new DateOnly(2026, 1, 15);
        var description = "Payment to vendor";
        var amount = 100.00m;

        repo.Setup(r => r.UpdateTwoSplitAsync(
                txId,
                bookedOn,
                description,
                payeeId,
                fromAccountId,
                toAccountId,
                amount,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateTransactionHandler(uow.Object, repo.Object);

        var request = new UpdateTransactionRequest(
            Id: txId,
            BookedOn: bookedOn,
            Description: description,
            PayeeId: payeeId,
            FromAccountId: fromAccountId,
            ToAccountId: toAccountId,
            Amount: amount);

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.UpdateTwoSplitAsync(
            txId,
            bookedOn,
            description,
            payeeId,
            fromAccountId,
            toAccountId,
            amount,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
