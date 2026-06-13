using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Handlers;
using FamilyFinances.Domain.Ledger.Accounts;
using FluentAssertions;
using Moq;

namespace FamilyFinances.Application.Tests.Ledger.Accounts;

public sealed class ListAccountsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsDtos()
    {
        var repo = new Mock<IAccountRepository>(MockBehavior.Strict);

        var checkingKind = AccountKindCatalog.CreateSystem("checking", "Checking", 10, AccountNature.Asset, AccountKind.Checking);
        var cashKind = AccountKindCatalog.CreateSystem("cash", "Cash", 40, AccountNature.Asset, AccountKind.Cash);

        var a1 = CreateAccountWithKindCatalog("Bank", AccountNature.Asset, checkingKind, new DateOnly(2026, 1, 1));
        var a2 = CreateAccountWithKindCatalog("Cash", AccountNature.Asset, cashKind, new DateOnly(2026, 1, 1));

        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { a1, a2 });

        var handler = new ListAccountsHandler(repo.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain(new[] { "Bank", "Cash" });

        repo.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    private static Account CreateAccountWithKindCatalog(
        string name,
        AccountNature nature,
        AccountKindCatalog kind,
        DateOnly openedOn)
    {
        var account = Account.Create(name, nature, kind.Id, kind.LegacyKind, openedOn);
        typeof(Account).GetProperty(nameof(Account.KindCatalog))!.SetValue(account, kind);
        return account;
    }
}
