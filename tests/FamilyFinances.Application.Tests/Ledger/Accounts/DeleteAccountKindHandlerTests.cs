using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class DeleteAccountKindHandlerTests
{
    [Fact]
    public async Task HandleAsync_DeletesCustomKind_WhenUnused()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var customKind = AccountKindCatalog.CreateCustom("travel", "Travel", 1000, AccountNature.Expense);

        repo.Setup(r => r.GetKindByIdAsync(customKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customKind);
        repo.Setup(r => r.IsKindReferencedByAccountsAsync(customKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repo.Setup(r => r.RemoveKind(customKind));
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new DeleteAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(customKind.Id.Value, CancellationToken.None);

        result.Should().BeTrue();
        repo.VerifyAll();
        uow.VerifyAll();
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenKindDoesNotExist()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetKindByIdAsync(It.IsAny<AccountKindCatalogId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountKindCatalog?)null);

        var sut = new DeleteAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeFalse();
        repo.VerifyAll();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenDeletingSystemKind()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var systemKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        repo.Setup(r => r.GetKindByIdAsync(systemKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(systemKind);

        var sut = new DeleteAccountKindHandler(repo.Object, uow.Object);

        var act = async () => await sut.HandleAsync(systemKind.Id.Value, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("System account kinds cannot be deleted.");
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenKindIsInUse()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var customKind = AccountKindCatalog.CreateCustom("travel", "Travel", 1000, AccountNature.Expense);

        repo.Setup(r => r.GetKindByIdAsync(customKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customKind);
        repo.Setup(r => r.IsKindReferencedByAccountsAsync(customKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new DeleteAccountKindHandler(repo.Object, uow.Object);

        var act = async () => await sut.HandleAsync(customKind.Id.Value, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Account kind cannot be deleted because it is in use.");
    }
}
