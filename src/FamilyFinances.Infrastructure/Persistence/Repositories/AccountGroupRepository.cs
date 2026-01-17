using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Domain.Ledger.AccountGroups;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class AccountGroupRepository : IAccountGroupRepository
{
    private readonly LedgerDbContext _db;

    public AccountGroupRepository(LedgerDbContext db) => _db = db;

    public Task AddAsync(AccountGroup group, CancellationToken ct)
        => _db.AccountGroups.AddAsync(group, ct).AsTask();

    public Task<AccountGroup?> GetByIdAsync(AccountGroupId id, CancellationToken ct)
        => _db.AccountGroups
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<AccountGroup?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct)
        => _db.AccountGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName, ct);

    public async Task<IReadOnlyList<AccountGroup>> ListAsync(CancellationToken ct)
        => await _db.AccountGroups
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public void Remove(AccountGroup group)
        => _db.AccountGroups.Remove(group);
}
