using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Domain.Tests.Ledger;

public sealed class TransactionTests
{
    [Fact]
    public void Create_AllowsBalancedSplits()
    {
        var bank = AccountId.New();
        var groceries = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(bank, new Money(-5000), "Payment"),     // -50.00
            TransactionSplit.Create(groceries, new Money(5000), "Expense")  // +50.00
        };

        var tx = Transaction.Create(new DateOnly(2026, 1, 2), "Groceries", splits);

        Assert.NotEqual(Guid.Empty, tx.Id.Value);
        Assert.Equal(new DateOnly(2026, 1, 2), tx.BookedOn);
        Assert.Equal("Groceries", tx.Description);
        Assert.Equal(2, tx.Splits.Count);
    }

    [Fact]
    public void Create_RejectsLessThanTwoSplits()
    {
        var bank = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(bank, new Money(100))
        };

        Assert.Throws<DomainException>(() =>
            Transaction.Create(new DateOnly(2026, 1, 2), "Invalid", splits));
    }

    [Fact]
    public void Create_RejectsUnbalancedSplits()
    {
        var a = AccountId.New();
        var b = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(a, new Money(1000)),
            TransactionSplit.Create(b, new Money(200)) // sum = 1200
        };

        Assert.Throws<DomainException>(() =>
            Transaction.Create(new DateOnly(2026, 1, 2), "Unbalanced", splits));
    }

    [Fact]
    public void Split_RejectsEmptyAccountId()
    {
        var empty = new AccountId(Guid.Empty);

        Assert.Throws<DomainException>(() =>
            TransactionSplit.Create(empty, new Money(100)));
    }

    [Fact]
    public void Create_RejectsEmptyDescription()
    {
        var a = AccountId.New();
        var b = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(a, new Money(-100)),
            TransactionSplit.Create(b, new Money(100))
        };

        Assert.Throws<DomainException>(() =>
            Transaction.Create(new DateOnly(2026, 1, 2), "   ", splits));
    }

    [Fact]
    public void Create_AllowsPayeeId()
    {
        var bank = AccountId.New();
        var groceries = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(bank, new Money(-5000)),
            TransactionSplit.Create(groceries, new Money(5000))
        };

        var payeeId = PayeeId.New();

        var tx = Transaction.Create(new DateOnly(2026, 1, 2), "Groceries", splits, payeeId);

        Assert.Equal(payeeId, tx.PayeeId);
    }

    [Fact]
    public void Create_RejectsEmptyPayeeId()
    {
        var bank = AccountId.New();
        var groceries = AccountId.New();

        var splits = new[]
        {
            TransactionSplit.Create(bank, new Money(-5000)),
            TransactionSplit.Create(groceries, new Money(5000))
        };

        var emptyPayeeId = new PayeeId(Guid.Empty);

        Assert.Throws<DomainException>(() =>
            Transaction.Create(new DateOnly(2026, 1, 2), "Groceries", splits, emptyPayeeId));
    }
}
