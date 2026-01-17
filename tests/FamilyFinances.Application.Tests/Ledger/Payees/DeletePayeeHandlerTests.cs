using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Payees;

public sealed class DeletePayeeHandlerTests
{
    [Fact]
    public async Task HandleAsync_DeletesPayee_AndPersistsChanges()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Payee");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.IsReferencedByTransactionsAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.Remove(existingPayee))
            .Verifiable();

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePayeeHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(payeeId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedByTransactionsAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.Remove(existingPayee), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenPayeeNotFound()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        var handler = new DeletePayeeHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(payeeId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);

        // No other operations should be performed
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenPayeeIsReferencedByTransactions()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Referenced Payee");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.IsReferencedByTransactionsAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeletePayeeHandler(repo.Object, uow.Object);

        // Act
        var act = async () => await handler.HandleAsync(payeeId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*referenced by transactions*");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedByTransactionsAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);

        // No deletion or persistence should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ChecksReferences_BeforeDeletion()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Test Payee");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var sequence = new MockSequence();

        // Ensure operations happen in the correct order
        repo.InSequence(sequence)
            .Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.InSequence(sequence)
            .Setup(r => r.IsReferencedByTransactionsAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.InSequence(sequence)
            .Setup(r => r.Remove(existingPayee));

        uow.InSequence(sequence)
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeletePayeeHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(payeeId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedByTransactionsAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.Remove(existingPayee), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
