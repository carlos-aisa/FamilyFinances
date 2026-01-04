using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Application.Ledger.AccountGroups.Handlers;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.AccountGroups;

public sealed class CreateAccountGroupHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesAccountGroup_AndPersistsIt()
    {
        var repo = new Mock<IAccountGroupRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        repo.Setup(r => r.GetByNormalizedNameAsync("HOME EXPENSES", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountGroup?)null);

        repo.Setup(r => r.AddAsync(It.IsAny<AccountGroup>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccountGroupHandler(repo.Object, uow.Object);

        var request = new CreateAccountGroupRequest("Home Expenses", "All home related");

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Home Expenses");
        result.Description.Should().Be("All home related");

        repo.Verify(r => r.GetByNormalizedNameAsync("HOME EXPENSES", It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.AddAsync(It.Is<AccountGroup>(g =>
            g.Name == "Home Expenses" &&
            g.Description == "All home related"), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenNameAlreadyExists()
    {
        var repo = new Mock<IAccountGroupRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);

        var existing = AccountGroup.Create("Home Expenses", "Existing");

        repo.Setup(r => r.GetByNormalizedNameAsync("HOME EXPENSES", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new CreateAccountGroupHandler(repo.Object, uow.Object);

        var request = new CreateAccountGroupRequest("home expenses", "New");

        var act = async () => await handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }
}
