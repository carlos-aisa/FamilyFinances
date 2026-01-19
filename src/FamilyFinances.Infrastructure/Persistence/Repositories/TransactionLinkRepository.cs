using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class TransactionLinkRepository : ITransactionLinkRepository
{
    private readonly LedgerDbContext _db;

    public TransactionLinkRepository(LedgerDbContext db) => _db = db;

    public Task AddAsync(TransactionLink link, CancellationToken ct)
        => _db.TransactionLinks.AddAsync(link, ct).AsTask();

    public async Task<IReadOnlyList<TransactionLink>> GetLinksForTransactionAsync(
        TransactionId transactionId, 
        CancellationToken ct)
    {
        return await _db.TransactionLinks
            .AsNoTracking()
            .Where(l => l.SourceTransactionId == transactionId || l.TargetTransactionId == transactionId)
            .ToListAsync(ct);
    }
}

