using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Handlers;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Payees;

public sealed class RenamePayeeHandlerTests
{
    [Fact]
    public async Task HandleAsync_RenamesPayee_AndPersistsChanges()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Name");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.GetByNormalizedNameAsync("NEW NAME", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("New Name");

        // Act
        var result = await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingPayee.Name.Should().Be("New Name");
        existingPayee.NormalizedName.Should().Be("NEW NAME");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync("NEW NAME", It.IsAny<CancellationToken>()), Times.Once);
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

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("New Name");

        // Act
        var result = await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);

        // No other operations should be performed
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenNameAlreadyExistsForDifferentPayee()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Name");
        var otherPayee = Payee.Create("Duplicate Name");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.GetByNormalizedNameAsync("DUPLICATE NAME", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherPayee);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("Duplicate Name");

        // Act
        var act = async () => await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync("DUPLICATE NAME", It.IsAny<CancellationToken>()), Times.Once);

        // No persistence should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_AllowsRenamingToSameName()
    {
        // Arrange
        var existingPayee = Payee.Create("Same Name");
        var payeeId = existingPayee.Id.Value; // Use the actual ID from the created payee
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        // Return the same payee when checking for duplicates
        repo.Setup(r => r.GetByNormalizedNameAsync("SAME NAME", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("Same Name");

        // Act
        var result = await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync("SAME NAME", It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenNewNameIsEmpty()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Name");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.GetByNormalizedNameAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("   "); // Empty/whitespace name

        // Act
        var act = async () => await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*required*");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync("", It.IsAny<CancellationToken>()), Times.Once);

        // No persistence should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenNewNameIsTooLong()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Name");
        var tooLongName = new string('A', 201); // More than 200 characters
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.GetByNormalizedNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest(tooLongName);

        // Act
        var act = async () => await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*too long*");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        // No persistence should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);

        // Act
        var act = async () => await handler.HandleAsync(payeeId, null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();

        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_TrimsWhitespaceFromNewName()
    {
        // Arrange
        var payeeId = Guid.NewGuid();
        var existingPayee = Payee.Create("Old Name");
        
        var repo = new Mock<IPayeeRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<PayeeId>(id => id.Value == payeeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayee);

        repo.Setup(r => r.GetByNormalizedNameAsync("TRIMMED NAME", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payee?)null);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RenamePayeeHandler(repo.Object, uow.Object);
        var request = new RenamePayeeRequest("  Trimmed Name  ");

        // Act
        var result = await handler.HandleAsync(payeeId, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingPayee.Name.Should().Be("Trimmed Name");
        existingPayee.NormalizedName.Should().Be("TRIMMED NAME");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<PayeeId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetByNormalizedNameAsync("TRIMMED NAME", It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
