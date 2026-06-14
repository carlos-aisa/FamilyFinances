using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class CreateAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAccount_AndPersistsIt()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var checkingKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        repo.Setup(r => r.GetKindByLegacyAndNatureAsync(AccountKind.Checking, AccountNature.Asset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkingKind);

        repo.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 2));

        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Main Bank");
        result.Nature.Should().Be(AccountNature.Asset);
        result.Kind.Should().Be(AccountKind.Checking);
        result.KindId.Should().Be(checkingKind.Id.Value);
        result.KindKey.Should().Be("checking");
        result.KindName.Should().Be("Checking");
        result.OpenedOn.Should().Be(new DateOnly(2026, 1, 2));
        result.IsClosed.Should().BeFalse();
        result.ClosedOn.Should().BeNull();

        repo.Verify(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.GetKindByLegacyAndNatureAsync(AccountKind.Checking, AccountNature.Asset, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.AddAsync(It.Is<Account>(a =>
            a.Name == "Main Bank" &&
            a.Nature == AccountNature.Asset &&
            a.KindId == checkingKind.Id &&
            a.OpenedOn == new DateOnly(2026, 1, 2)), It.IsAny<CancellationToken>()), Times.Once);

        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_PropagatesDomainException_ForInvalidName()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(r => r.GetKindByLegacyAndNatureAsync(AccountKind.Checking, AccountNature.Asset, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking));

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "   ",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 2));

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenNameAlreadyExists()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 2));

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Account name already exists.");

        repo.Verify(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenSelectedKindDoesNotExist()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var kindId = Guid.NewGuid();
        repo.Setup(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.GetKindByIdAsync(new AccountKindCatalogId(kindId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountKindCatalog?)null);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Other,
            OpenedOn: new DateOnly(2026, 1, 2),
            KindId: kindId);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Selected account kind does not exist.");
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenSelectedKindIsInactive()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var custom = AccountKindCatalog.CreateCustom("my-kind", "My Kind", 1000, AccountNature.Asset);
        custom.Deactivate();

        repo.Setup(r => r.GetKindByIdAsync(custom.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(custom);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Other,
            OpenedOn: new DateOnly(2026, 1, 2),
            KindId: custom.Id.Value);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Selected account kind is inactive.");
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenSelectedKindIsNotCompatibleWithNature()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsByNormalizedNameAsync("MAIN BANK", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var incomeKind = AccountKindCatalog.CreateCustom("income-other", "Income Other", 1000, AccountNature.Income);

        repo.Setup(r => r.GetKindByIdAsync(incomeKind.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(incomeKind);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountRequest(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Other,
            OpenedOn: new DateOnly(2026, 1, 2),
            KindId: incomeKind.Id.Value);

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Selected account kind is not compatible with account nature.");
    }
}
