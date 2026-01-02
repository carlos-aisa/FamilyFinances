using FamilyFinances.Application.Abstractions;
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
}
