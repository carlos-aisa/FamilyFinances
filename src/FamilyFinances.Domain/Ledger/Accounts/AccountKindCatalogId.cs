namespace FamilyFinances.Domain.Ledger.Accounts;

public readonly record struct AccountKindCatalogId(Guid Value)
{
    public static AccountKindCatalogId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}