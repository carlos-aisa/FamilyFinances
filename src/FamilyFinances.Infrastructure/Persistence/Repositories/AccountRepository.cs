using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly LedgerDbContext _db;

    public AccountRepository(LedgerDbContext db) => _db = db;

    public Task AddAsync(Account account, CancellationToken ct)
        => _db.Accounts.AddAsync(account, ct).AsTask();

    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken ct)
        => await _db.Accounts.AsNoTracking().OrderBy(a => a.Name).ToListAsync(ct);

    public Task<Account?> GetByIdAsync(AccountId id, CancellationToken ct)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, AccountId? excludingId, CancellationToken ct)
    {
        var query = _db.Accounts
            .Where(a => a.NormalizedName == normalizedName && !a.IsClosed);

        if (excludingId is not null)
            query = query.Where(a => a.Id != excludingId);

        return await query.AnyAsync(ct);
    }

    public Task<Account?> GetByIdForUpdateAsync(AccountId id, CancellationToken ct)
    => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> IsReferencedBySplitsAsync(AccountId id, CancellationToken ct)
        => _db.TransactionSplits.AnyAsync(s => s.AccountId == id, ct);

    public void Remove(Account account)
        => _db.Accounts.Remove(account);

}
