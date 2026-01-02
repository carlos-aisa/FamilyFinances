using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Tests.Accounts;

public sealed class AccountTests
{
    [Fact]
    public void Create_AssignsId_AndSetsFields()
    {
        var openedOn = new DateOnly(2026, 1, 2);

        var account = Account.Create("Main Bank", AccountNature.Asset, AccountKind.Checking, openedOn);

        Assert.NotEqual(default, account.Id.Value);
        Assert.Equal("Main Bank", account.Name);
        Assert.Equal(AccountNature.Asset, account.Nature);
        Assert.Equal(AccountKind.Checking, account.Kind);
        Assert.Equal(openedOn, account.OpenedOn);
        Assert.False(account.IsClosed);
        Assert.Null(account.ClosedOn);
        Assert.Equal(Currency.EUR, account.Currency);
    }

    [Fact]
    public void Create_RejectsEmptyName()
    {
        var openedOn = new DateOnly(2026, 1, 2);

        Assert.Throws<DomainException>(() =>
            Account.Create("   ", AccountNature.Asset, AccountKind.Checking, openedOn));
    }

    [Fact]
    public void Rename_UpdatesName()
    {
        var openedOn = new DateOnly(2026, 1, 2);
        var account = Account.Create("Old", AccountNature.Asset, AccountKind.Checking, openedOn);

        account.Rename("New Name");

        Assert.Equal("New Name", account.Name);
    }

    [Fact]
    public void Rename_RejectsEmptyName()
    {
        var openedOn = new DateOnly(2026, 1, 2);
        var account = Account.Create("Valid", AccountNature.Asset, AccountKind.Checking, openedOn);

        Assert.Throws<DomainException>(() => account.Rename("  "));
    }

    [Fact]
    public void Close_SetsClosedFields()
    {
        var openedOn = new DateOnly(2026, 1, 1);
        var account = Account.Create("Main", AccountNature.Asset, AccountKind.Checking, openedOn);

        account.Close(new DateOnly(2026, 2, 1));

        Assert.True(account.IsClosed);
        Assert.Equal(new DateOnly(2026, 2, 1), account.ClosedOn);
    }

    [Fact]
    public void Close_RejectsDateEarlierThanOpenedOn()
    {
        var openedOn = new DateOnly(2026, 2, 1);
        var account = Account.Create("Main", AccountNature.Asset, AccountKind.Checking, openedOn);

        Assert.Throws<DomainException>(() =>
            account.Close(new DateOnly(2026, 1, 31)));
    }

    [Fact]
    public void Reopen_ClearsClosedFields()
    {
        var openedOn = new DateOnly(2026, 1, 1);
        var account = Account.Create("Main", AccountNature.Asset, AccountKind.Checking, openedOn);

        account.Close(new DateOnly(2026, 2, 1));
        account.Reopen();

        Assert.False(account.IsClosed);
        Assert.Null(account.ClosedOn);
    }
}
