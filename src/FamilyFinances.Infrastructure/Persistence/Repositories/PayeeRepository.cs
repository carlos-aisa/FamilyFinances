using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Domain.Ledger.Payees;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

internal sealed class PayeeRepository : IPayeeRepository
{
    private readonly LedgerDbContext _db;

    public PayeeRepository(LedgerDbContext db) => _db = db;

    public Task<Payee?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct)
        => _db.Payees
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.NormalizedName == normalizedName, ct);

    public async Task<IReadOnlyList<Payee>> ListAsync(CancellationToken ct)
        => await _db.Payees
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public Task AddAsync(Payee payee, CancellationToken ct)
        => _db.Payees.AddAsync(payee, ct).AsTask();

    public Task<Payee?> GetByIdAsync(PayeeId id, CancellationToken ct) 
        => _db.Payees
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payee?> GetByIdForUpdateAsync(PayeeId id, CancellationToken ct)
    => _db.Payees.FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Remove(Payee payee)
        => _db.Payees.Remove(payee);

    public Task<bool> IsReferencedByTransactionsAsync(PayeeId id, CancellationToken ct)
        => _db.Transactions.AnyAsync(t => t.PayeeId == id, ct);

}
