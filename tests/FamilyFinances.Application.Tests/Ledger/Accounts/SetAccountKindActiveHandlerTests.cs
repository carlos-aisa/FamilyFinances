using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class SetAccountKindActiveHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_WhenSystemKindIsDeactivated()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var systemKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        repo.Setup(r => r.GetKindByIdAsync(systemKind.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(systemKind);

        var sut = new SetAccountKindActiveHandler(repo.Object, uow.Object);

        var act = async () => await sut.HandleAsync(systemKind.Id.Value, new SetAccountKindActiveRequest(false), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("System account kinds cannot be deactivated.");
    }

    [Fact]
    public async Task HandleAsync_UpdatesCustomKindActiveFlag()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var customKind = AccountKindCatalog.CreateCustom("travel", "Travel", 1000, AccountNature.Expense);
        repo.Setup(r => r.GetKindByIdAsync(customKind.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customKind);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new SetAccountKindActiveHandler(repo.Object, uow.Object);

        await sut.HandleAsync(customKind.Id.Value, new SetAccountKindActiveRequest(false), CancellationToken.None);
        customKind.IsActive.Should().BeFalse();

        await sut.HandleAsync(customKind.Id.Value, new SetAccountKindActiveRequest(true), CancellationToken.None);
        customKind.IsActive.Should().BeTrue();
    }
}
