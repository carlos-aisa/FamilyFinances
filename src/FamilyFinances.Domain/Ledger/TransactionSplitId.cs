namespace FamilyFinances.Domain.Ledger;

public readonly record struct TransactionSplitId(Guid Value)
{
    public static TransactionSplitId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
