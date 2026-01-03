namespace FamilyFinances.Domain.Ledger.AccountGroups;

public readonly record struct AccountGroupId(Guid Value)
{
    public static AccountGroupId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
