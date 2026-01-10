using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class DeleteAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_DeletesAccount_AndPersistsChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = Account.Create(name: "Test Account", nature: AccountNature.Asset, kind: AccountKind.Cash, openedOn: DateOnly.FromDateTime(DateTime.UtcNow));
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        repo.Setup(r => r.IsReferencedBySplitsAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.Remove(existingAccount))
            .Verifiable();

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedBySplitsAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.Remove(existingAccount), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);

        // No other operations should be performed
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenAccountIsReferencedBySplits()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = Account.Create(name: "Referenced Account", nature: AccountNature.Asset, kind: AccountKind.Cash, openedOn: DateOnly.FromDateTime(DateTime.UtcNow));
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        repo.Setup(r => r.IsReferencedBySplitsAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var act = async () => await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*referenced by transactions*");

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedBySplitsAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);

        // No deletion or persistence should happen
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_SuggestsClosingAccount_WhenReferenced()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = Account.Create(name: "Active Account", nature: AccountNature.Asset, kind: AccountKind.Cash, openedOn: DateOnly.FromDateTime(DateTime.UtcNow));
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        repo.Setup(r => r.IsReferencedBySplitsAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var act = async () => await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Close it instead*");
    }

    [Fact]
    public async Task HandleAsync_ChecksReferences_BeforeDeletion()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var existingAccount = Account.Create(name: "Test Account", nature: AccountNature.Asset, kind: AccountKind.Cash, openedOn: DateOnly.FromDateTime(DateTime.UtcNow));
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var sequence = new MockSequence();

        // Ensure operations happen in the correct order
        repo.InSequence(sequence)
            .Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        repo.InSequence(sequence)
            .Setup(r => r.IsReferencedBySplitsAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.InSequence(sequence)
            .Setup(r => r.Remove(existingAccount));

        uow.InSequence(sequence)
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        repo.Verify(r => r.GetByIdForUpdateAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.IsReferencedBySplitsAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.Remove(existingAccount), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AllowsDeletion_WhenAccountHasNoTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var unusedAccount = Account.Create(name: "Unused Category", nature: AccountNature.Expense, kind: AccountKind.ExpenseCategory, openedOn: DateOnly.FromDateTime(DateTime.UtcNow));
        
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unusedAccount);

        repo.Setup(r => r.IsReferencedBySplitsAsync(It.Is<AccountId>(id => id.Value == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.Remove(unusedAccount));

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repo.Verify(r => r.Remove(unusedAccount), Times.Once);
    }
}
