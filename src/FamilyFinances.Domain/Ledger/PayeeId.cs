namespace FamilyFinances.Domain.Ledger;

public readonly record struct PayeeId(Guid Value)
{
    public static PayeeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
