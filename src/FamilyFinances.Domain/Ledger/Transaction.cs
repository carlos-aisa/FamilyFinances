using FamilyFinances.Domain.Common;

namespace FamilyFinances.Domain.Ledger;

public sealed class Transaction
{
    private readonly List<TransactionSplit> _splits = new();

    public TransactionId Id { get; }
    public DateOnly BookedOn { get; }
    public string Description { get; }

    public PayeeId? PayeeId { get; }

    public IReadOnlyList<TransactionSplit> Splits => _splits;

#pragma warning disable CS8618
    private Transaction() { } // For EF Core
#pragma warning restore CS8618

    private Transaction(
        TransactionId id,
        DateOnly bookedOn,
        string description,
        PayeeId? payeeId, 
        IEnumerable<TransactionSplit> splits)
    {
        Id = id;
        BookedOn = bookedOn;
        Description = description;
        PayeeId = payeeId; 
        _splits.AddRange(splits);
    }

    // Keep the existing API to avoid touching all callers immediately
    public static Transaction Create(DateOnly bookedOn, string description, IEnumerable<TransactionSplit> splits)
        => Create(bookedOn, description, splits, null);

    // NEW overload
    public static Transaction Create(
        DateOnly bookedOn,
        string description,
        IEnumerable<TransactionSplit> splits,
        PayeeId? payeeId)
    {
        if (bookedOn == default)
            throw new DomainException("BookedOn date is required.");

        description = (description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Transaction description is required.");

        if (splits is null)
            throw new DomainException("Splits are required.");

        if (payeeId is not null && payeeId.Value.Value == Guid.Empty)
            throw new DomainException("PayeeId cannot be empty.");

        var list = splits.ToList();

        if (list.Count < 2)
            throw new DomainException("A transaction must have at least two splits.");

        var total = 0L;
        checked
        {
            foreach (var s in list)
            {
                if (s is null)
                    throw new DomainException("Split cannot be null.");

                total += s.Amount.Cents;
            }
        }

        if (total != 0)
            throw new DomainException("Transaction splits must be balanced (sum must be zero).");

        return new Transaction(TransactionId.New(), bookedOn, description, payeeId, list);
    }
}
