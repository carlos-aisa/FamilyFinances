using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class RenameAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_RenamesAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = Account.Create(
            name: "Old Name",
            nature: AccountNature.Asset,
            kind: AccountKind.Checking,
            openedOn: new DateOnly(2026, 1, 1));

        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("NEW NAME", It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RenameAccountHandler(repo.Object, uow.Object);
        var request = new RenameAccountRequest("New Name");

        // Act
        var result = await handler.HandleAsync(accountId, request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        account.Name.Should().Be("New Name");
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.ExistsByNormalizedNameAsync("NEW NAME", It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
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

        repo.Setup(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var handler = new RenameAccountHandler(repo.Object, uow.Object);
        var request = new RenameAccountRequest("New Name");

        // Act
        var result = await handler.HandleAsync(accountId, request, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenNameAlreadyExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = Account.Create(
            name: "Old Name",
            nature: AccountNature.Asset,
            kind: AccountKind.Checking,
            openedOn: new DateOnly(2026, 1, 1));

        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("DUPLICATE NAME", It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RenameAccountHandler(repo.Object, uow.Object);
        var request = new RenameAccountRequest("Duplicate Name");

        // Act
        var act = async () => await handler.HandleAsync(accountId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.ExistsByNormalizedNameAsync("DUPLICATE NAME", It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }
}
