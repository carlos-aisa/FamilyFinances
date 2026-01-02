using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Create;
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

        repo.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountCommand(
            Name: "Main Bank",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 2));

        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Main Bank");
        result.Nature.Should().Be(AccountNature.Asset);
        result.Kind.Should().Be(AccountKind.Checking);
        result.OpenedOn.Should().Be(new DateOnly(2026, 1, 2));
        result.IsClosed.Should().BeFalse();
        result.ClosedOn.Should().BeNull();

        repo.Verify(r => r.AddAsync(It.Is<Account>(a =>
            a.Name == "Main Bank" &&
            a.Nature == AccountNature.Asset &&
            a.Kind == AccountKind.Checking &&
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

        var handler = new CreateAccountHandler(repo.Object, uow.Object);

        var cmd = new CreateAccountCommand(
            Name: "   ",
            Nature: AccountNature.Asset,
            Kind: AccountKind.Checking,
            OpenedOn: new DateOnly(2026, 1, 2));

        var act = async () => await handler.HandleAsync(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
