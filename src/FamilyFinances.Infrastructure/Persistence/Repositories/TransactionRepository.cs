using FamilyFinances.Application.Abstractions;
using FamilyFinances.Application.Ledger;
using FamilyFinances.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly LedgerDbContext _db;

    public TransactionRepository(LedgerDbContext db) => _db = db;

    public Task AddAsync(Transaction transaction, CancellationToken ct)
        => _db.Transactions.AddAsync(transaction, ct).AsTask();

    public Task<Transaction?> GetByIdAsync(TransactionId id, CancellationToken ct)
        => _db.Transactions
            .AsNoTracking()
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
}
