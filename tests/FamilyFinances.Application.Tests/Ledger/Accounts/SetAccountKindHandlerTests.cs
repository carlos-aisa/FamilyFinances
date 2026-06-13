using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class SetAccountKindHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesAccountKind_WhenCompatibleAndActive()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var currentKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        var account = Account.Create("Main Bank", AccountNature.Asset, currentKind.Id, currentKind.LegacyKind, new DateOnly(2026, 1, 2));

        var targetKind = AccountKindCatalog.CreateCustom("broker", "Broker", 1000, AccountNature.Asset);

        repo.Setup(r => r.GetByIdForUpdateAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        repo.Setup(r => r.GetKindByIdAsync(targetKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetKind);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new SetAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(account.Id.Value, new SetAccountKindRequest(targetKind.Id.Value), CancellationToken.None);

        result.Should().BeTrue();
        account.KindId.Should().Be(targetKind.Id);
        account.Kind.Should().Be(AccountKind.Other);

        repo.VerifyAll();
        uow.VerifyAll();
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenAccountDoesNotExist()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdForUpdateAsync(It.IsAny<AccountId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var sut = new SetAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(Guid.NewGuid(), new SetAccountKindRequest(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
        repo.VerifyAll();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenKindNotCompatibleWithNature()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var currentKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        var account = Account.Create("Main Bank", AccountNature.Asset, currentKind.Id, currentKind.LegacyKind, new DateOnly(2026, 1, 2));

        var targetKind = AccountKindCatalog.CreateCustom("income-other", "Income Other", 1000, AccountNature.Income);

        repo.Setup(r => r.GetByIdForUpdateAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        repo.Setup(r => r.GetKindByIdAsync(targetKind.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetKind);

        var sut = new SetAccountKindHandler(repo.Object, uow.Object);

        var act = async () => await sut.HandleAsync(account.Id.Value, new SetAccountKindRequest(targetKind.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Selected account kind is not compatible with account nature.");
    }
}
