using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class CloseAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ClosesAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = Account.Create(
            name: "Test Account",
            nature: AccountNature.Asset,
            kind: AccountKind.Checking,
            openedOn: new DateOnly(2026, 1, 1));

        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CloseAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        account.IsClosed.Should().BeTrue();
        account.ClosedOn.Should().NotBeNull();
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
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

        var handler = new CloseAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }
}
