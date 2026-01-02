using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger.Transactions;

public sealed class TransactionLink
{
    public Guid Id { get; }
    public TransactionId SourceTransactionId { get; }
    public TransactionId TargetTransactionId { get; }
    public TransactionLinkType Type { get; }
    public DateOnly LinkedOn { get; }

#pragma warning disable CS8618
    private TransactionLink() { } // For EF Core
#pragma warning restore CS8618

    private TransactionLink(
        Guid id,
        TransactionId sourceTransactionId,
        TransactionId targetTransactionId,
        TransactionLinkType type,
        DateOnly linkedOn)
    {
        Id = id;
        SourceTransactionId = sourceTransactionId;
        TargetTransactionId = targetTransactionId;
        Type = type;
        LinkedOn = linkedOn;
    }

    public static TransactionLink Create(
        TransactionId sourceTransactionId,
        TransactionId targetTransactionId,
        TransactionLinkType type,
        DateOnly linkedOn)
    {
        if (sourceTransactionId.Value == Guid.Empty)
            throw new DomainException("SourceTransactionId is required.");

        if (targetTransactionId.Value == Guid.Empty)
            throw new DomainException("TargetTransactionId is required.");

        if (linkedOn == default)
            throw new DomainException("LinkedOn date is required.");

        if (sourceTransactionId.Value == targetTransactionId.Value)
            throw new DomainException("A transaction cannot be linked to itself.");

        return new TransactionLink(Guid.NewGuid(), sourceTransactionId, targetTransactionId, type, linkedOn);
    }
}
