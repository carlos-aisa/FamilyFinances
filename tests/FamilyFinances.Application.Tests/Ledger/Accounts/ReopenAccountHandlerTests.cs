using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class ReopenAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReopensAccount_WhenAccountIsClosed()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = Account.Create(
            name: "Test Account",
            nature: AccountNature.Asset,
            kind: AccountKind.Checking,
            openedOn: new DateOnly(2026, 1, 1));
        
        account.Close(DateOnly.FromDateTime(DateTime.Today));

        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new ReopenAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        account.IsClosed.Should().BeFalse();
        account.ClosedOn.Should().BeNull();
        
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

        var handler = new ReopenAccountHandler(repo.Object, uow.Object);

        // Act
        var result = await handler.HandleAsync(accountId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        
        repo.Verify(r => r.GetByIdAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }
}
