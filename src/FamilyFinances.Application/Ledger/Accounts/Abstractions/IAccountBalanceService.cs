using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Abstractions;

/// <summary>
/// Service for computing account balances.
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// Computes the balance of an account as of a specific date (inclusive).
    /// Balance = sum of all split amounts for the account where BookedOn <= asOfDate.
    /// </summary>
    Task<decimal> ComputeBalanceAsOfAsync(AccountId accountId, DateOnly asOfDate, CancellationToken ct);
}
