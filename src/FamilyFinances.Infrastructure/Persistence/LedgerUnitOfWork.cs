using FamilyFinances.Application.Abstractions;

namespace FamilyFinances.Infrastructure.Persistence;

public sealed class LedgerUnitOfWork : ILedgerUnitOfWork
{
    private readonly LedgerDbContext _db;

    public LedgerUnitOfWork(LedgerDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
