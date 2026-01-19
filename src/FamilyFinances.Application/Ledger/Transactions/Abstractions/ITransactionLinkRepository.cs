using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Abstractions;

public interface ITransactionLinkRepository
{
    Task AddAsync(TransactionLink link, CancellationToken ct);
    Task<IReadOnlyList<TransactionLink>> GetLinksForTransactionAsync(TransactionId transactionId, CancellationToken ct);
}