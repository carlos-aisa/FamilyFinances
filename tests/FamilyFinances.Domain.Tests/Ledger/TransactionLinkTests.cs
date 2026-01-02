using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Domain.Tests.Ledger;

public sealed class TransactionLinkTests
{
    [Fact]
    public void Create_AllowsValidLink()
    {
        var source = TransactionId.New();
        var target = TransactionId.New();

        var link = TransactionLink.Create(source, target, TransactionLinkType.Refund, new DateOnly(2026, 1, 2));

        Assert.NotEqual(Guid.Empty, link.Id);
        Assert.Equal(source, link.SourceTransactionId);
        Assert.Equal(target, link.TargetTransactionId);
        Assert.Equal(TransactionLinkType.Refund, link.Type);
        Assert.Equal(new DateOnly(2026, 1, 2), link.LinkedOn);
    }

    [Fact]
    public void Create_RejectsSelfLink()
    {
        var id = TransactionId.New();

        Assert.Throws<DomainException>(() =>
            TransactionLink.Create(id, id, TransactionLinkType.Reversal, new DateOnly(2026, 1, 2)));
    }

    [Fact]
    public void EnsureNoDuplicates_RejectsDuplicates()
    {
        var a = TransactionId.New();
        var b = TransactionId.New();

        var link1 = TransactionLink.Create(a, b, TransactionLinkType.Adjustment, new DateOnly(2026, 1, 2));
        var link2 = TransactionLink.Create(a, b, TransactionLinkType.Adjustment, new DateOnly(2026, 1, 3)); // same key

        var links = new[] { link1, link2 };

        Assert.Throws<DomainException>(() => TransactionLinkSet.EnsureNoDuplicates(links));
    }

    [Fact]
    public void EnsureNoDuplicates_AllowsDifferentTypes()
    {
        var a = TransactionId.New();
        var b = TransactionId.New();

        var link1 = TransactionLink.Create(a, b, TransactionLinkType.Refund, new DateOnly(2026, 1, 2));
        var link2 = TransactionLink.Create(a, b, TransactionLinkType.Reversal, new DateOnly(2026, 1, 2));

        TransactionLinkSet.EnsureNoDuplicates(new[] { link1, link2 });
    }
}
