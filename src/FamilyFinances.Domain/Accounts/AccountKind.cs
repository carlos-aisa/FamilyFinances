namespace FamilyFinances.Domain.Accounts;

/// <summary>
/// User-facing classification of the account (UX / intent).
/// </summary>
public enum AccountKind
{
    Checking = 1,
    Savings = 2,
    CreditCard = 3,
    Cash = 4,
    Investment = 5,
    Other = 99
}
