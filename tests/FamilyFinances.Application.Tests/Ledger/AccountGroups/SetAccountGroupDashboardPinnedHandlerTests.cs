using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Handlers;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.AccountGroups;

public sealed class SetAccountGroupDashboardPinnedHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesExistingGroup()
    {
        var group = AccountGroup.Create("Home", null);
        var groups = new Mock<IAccountGroupRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);
        groups.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SetAccountGroupDashboardPinnedHandler(groups.Object, uow.Object);

        var updated = await handler.HandleAsync(group.Id.Value, new SetAccountGroupDashboardPinnedRequest(true), CancellationToken.None);

        updated.Should().BeTrue();
        group.IsDashboardPinned.Should().BeTrue();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenGroupDoesNotExist()
    {
        var groups = new Mock<IAccountGroupRepository>(MockBehavior.Strict);
        var uow = new Mock<ILedgerUnitOfWork>(MockBehavior.Strict);
        var id = Guid.NewGuid();
        groups.Setup(x => x.GetByIdAsync(new AccountGroupId(id), It.IsAny<CancellationToken>())).ReturnsAsync((AccountGroup?)null);

        var handler = new SetAccountGroupDashboardPinnedHandler(groups.Object, uow.Object);

        var updated = await handler.HandleAsync(id, new SetAccountGroupDashboardPinnedRequest(true), CancellationToken.None);

        updated.Should().BeFalse();
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
