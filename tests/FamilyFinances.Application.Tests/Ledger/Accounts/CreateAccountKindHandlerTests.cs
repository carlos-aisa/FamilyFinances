using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class CreateAccountKindHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesCustomKind_WithUniqueKey()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsKindByKeyAsync("tarjeta-de-viajes", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        repo.Setup(r => r.AddKindAsync(It.IsAny<AccountKindCatalog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new CreateAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(new CreateAccountKindRequest("Tarjeta de viajes", AccountNature.Expense), CancellationToken.None);

        result.Key.Should().Be("tarjeta-de-viajes");
        result.IsSystem.Should().BeFalse();
        result.IsActive.Should().BeTrue();
        result.Nature.Should().Be(AccountNature.Expense);

        repo.VerifyAll();
        uow.VerifyAll();
    }

    [Fact]
    public async Task HandleAsync_AppendsNumericSuffix_WhenKeyAlreadyExists()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.ExistsKindByKeyAsync("custom-kind", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repo.Setup(r => r.ExistsKindByKeyAsync("custom-kind-2", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.ListKindsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        repo.Setup(r => r.AddKindAsync(It.IsAny<AccountKindCatalog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new CreateAccountKindHandler(repo.Object, uow.Object);

        var result = await sut.HandleAsync(new CreateAccountKindRequest("Custom Kind", AccountNature.Expense), CancellationToken.None);

        result.Key.Should().Be("custom-kind-2");
    }
}
