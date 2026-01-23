using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Services;

/// <summary>
/// Service for computing account balances from transaction splits.
/// </summary>
public sealed class AccountBalanceService : IAccountBalanceService
{
    private readonly LedgerDbContext _db;

    public AccountBalanceService(LedgerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Computes the balance of an account as of a specific date (inclusive).
    /// Balance = sum of all split amounts for the account where BookedOn <= asOfDate.
    /// </summary>
    public async Task<decimal> ComputeBalanceAsOfAsync(
        AccountId accountId,
        DateOnly asOfDate,
        CancellationToken ct)
    {
        var splits = await (
            from s in _db.TransactionSplits.AsNoTracking()
            join t in _db.Transactions.AsNoTracking()
                on EF.Property<TransactionId>(s, "TransactionId") equals t.Id
            where s.AccountId == accountId
            where t.BookedOn <= asOfDate
            select s.Amount.Cents
        ).ToListAsync(ct);

        var balanceCents = splits.Sum();
        return balanceCents / 100m; // Convert cents to euros
    }
}
