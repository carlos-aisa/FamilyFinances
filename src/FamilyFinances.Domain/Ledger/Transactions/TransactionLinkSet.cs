using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Transactions;

public static class TransactionLinkSet
{
    public static void EnsureNoDuplicates(IEnumerable<TransactionLink> links)
    {
        if (links is null)
            return;

        var seen = new HashSet<(Guid Source, Guid Target, TransactionLinkType Type)>();

        foreach (var link in links)
        {
            if (link is null)
                throw new DomainException("Link cannot be null.");

            var key = (link.SourceTransactionId.Value, link.TargetTransactionId.Value, link.Type);

            if (!seen.Add(key))
                throw new DomainException("Duplicate transaction link detected.");
        }
    }
}
