namespace FamilyFinances.Domain.Ledger.Transactions;

public enum TransactionLinkType
{
    Refund = 1,
    Adjustment = 2,
    Reversal = 3,
    // Future: Split, Recurring, etc.
}
