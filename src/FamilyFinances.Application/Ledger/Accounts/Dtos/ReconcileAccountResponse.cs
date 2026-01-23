namespace FamilyFinances.Application.Ledger.Accounts.Dtos;

/// <summary>
/// Response from account reconciliation.
/// </summary>
public sealed record ReconcileAccountResponse(
    bool AdjustmentCreated,
    Guid? TransactionId,
    decimal ComputedBalance,
    decimal ActualBalance,
    decimal Difference,
    string Message
);
