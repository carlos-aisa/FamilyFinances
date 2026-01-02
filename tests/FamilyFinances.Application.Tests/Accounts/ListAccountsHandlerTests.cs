using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Accounts;
using FamilyFinances.Domain.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Accounts;

public sealed class ListAccountsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsDtos()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);

        var a1 = Account.Create("Bank", AccountNature.Asset, AccountKind.Checking, new DateOnly(2026, 1, 1));
        var a2 = Account.Create("Cash", AccountNature.Asset, AccountKind.Cash, new DateOnly(2026, 1, 1));

        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { a1, a2 });

        var handler = new ListAccountsHandler(repo.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain(new[] { "Bank", "Cash" });

        repo.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}
