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

    public Task AddKindAsync(AccountKindCatalog kind, CancellationToken ct)
        => _db.AccountKinds.AddAsync(kind, ct).AsTask();

    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken ct)
        => await _db.Accounts
            .AsNoTracking()
            .Include(a => a.KindCatalog)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AccountKindCatalog>> ListKindsAsync(bool includeInactive, CancellationToken ct)
    {
        var query = _db.AccountKinds.AsNoTracking();
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.IsSystem ? 0 : 1)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }

    public Task<AccountKindCatalog?> GetKindByIdAsync(AccountKindCatalogId id, CancellationToken ct)
        => _db.AccountKinds.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<AccountKindCatalog?> GetKindByKeyAsync(string key, CancellationToken ct)
        => _db.AccountKinds.FirstOrDefaultAsync(x => x.Key == key, ct);

    public Task<AccountKindCatalog?> GetKindByLegacyAsync(AccountKind legacyKind, CancellationToken ct)
        => _db.AccountKinds
            .Where(x => x.LegacyKind == legacyKind)
            .OrderByDescending(x => x.IsSystem)
            .ThenBy(x => x.SortOrder)
            .FirstOrDefaultAsync(ct);

    public Task<AccountKindCatalog?> GetKindByLegacyAndNatureAsync(AccountKind legacyKind, AccountNature nature, CancellationToken ct)
        => _db.AccountKinds
            .Where(x => x.LegacyKind == legacyKind && x.Nature == nature)
            .OrderByDescending(x => x.IsSystem)
            .ThenBy(x => x.SortOrder)
            .FirstOrDefaultAsync(ct);

    public Task<bool> IsKindReferencedByAccountsAsync(AccountKindCatalogId id, CancellationToken ct)
        => _db.Accounts.AnyAsync(a => a.KindId == id, ct);

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

    public async Task<bool> ExistsKindByKeyAsync(string key, AccountKindCatalogId? excludingId, CancellationToken ct)
    {
        var query = _db.AccountKinds.Where(x => x.Key == key);
        if (excludingId is not null)
            query = query.Where(x => x.Id != excludingId);

        return await query.AnyAsync(ct);
    }

    public Task<Account?> GetByIdForUpdateAsync(AccountId id, CancellationToken ct)
    => _db.Accounts
        .Include(a => a.KindCatalog)
        .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> IsReferencedBySplitsAsync(AccountId id, CancellationToken ct)
        => _db.TransactionSplits.AnyAsync(s => s.AccountId == id, ct);

    public void Remove(Account account)
        => _db.Accounts.Remove(account);

    public void RemoveKind(AccountKindCatalog kind)
        => _db.AccountKinds.Remove(kind);
}
