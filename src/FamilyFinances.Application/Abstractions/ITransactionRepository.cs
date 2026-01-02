using FamilyFinances.Domain.Ledger;

namespace FamilyFinances.Application.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<Transaction?> GetByIdAsync(TransactionId id, CancellationToken ct);
}
