using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Handlers;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.AccountGroups;

public sealed class ListAccountGroupsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAllGroups()
    {
        var repo = new Mock<IAccountGroupRepository>(MockBehavior.Strict);

        var groups = new List<AccountGroup>
        {
            AccountGroup.Create("Home", "Home expenses"),
            AccountGroup.Create("Work", "Work related"),
            AccountGroup.Create("Carlos", null)
        };

        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var handler = new ListAccountGroupsHandler(repo.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().Contain(g => g.Name == "Home" && g.Description == "Home expenses");
        result.Should().Contain(g => g.Name == "Work" && g.Description == "Work related");
        result.Should().Contain(g => g.Name == "Carlos" && g.Description == null);

        repo.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoGroups()
    {
        var repo = new Mock<IAccountGroupRepository>(MockBehavior.Strict);

        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AccountGroup>());

        var handler = new ListAccountGroupsHandler(repo.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }
}
