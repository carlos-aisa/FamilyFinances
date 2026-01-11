using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Ledger.Transactions;
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
            .Include(t=> t.Payee)
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Transaction>> ListAsync(int take, CancellationToken ct)
        => await _db.Transactions
            .AsNoTracking()
            .Include(t=> t.Payee)
            .Include(t => t.Splits).ThenInclude(s=> s.Account)
            .OrderByDescending(t => t.BookedOn)
            .ThenByDescending(t => t.Id)
            .Take(take)
            .ToListAsync(ct);

}
