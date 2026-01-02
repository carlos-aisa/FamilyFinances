namespace FamilyFinances.Domain.Ledger.Accounts;

/// <summary>
/// The nature of the account, used for financial reporting and accounting purposes.
/// </summary>
public enum AccountNature
{
    Asset = 1,
    Liability = 2,
    Income = 3,
    Expense = 4,
    Equity = 5
}
