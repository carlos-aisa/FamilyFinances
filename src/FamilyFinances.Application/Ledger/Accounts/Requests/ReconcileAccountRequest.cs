namespace FamilyFinances.Application.Ledger.Accounts.Requests;

/// <summary>
/// Request to reconcile an account balance by creating an adjustment transaction.
/// </summary>
public sealed record ReconcileAccountRequest(
    decimal ActualBalance,
    DateOnly AsOfDate,
    string? Note
);
