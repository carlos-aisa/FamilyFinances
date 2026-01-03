using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<Transaction?> GetByIdAsync(TransactionId id, CancellationToken ct);
}
