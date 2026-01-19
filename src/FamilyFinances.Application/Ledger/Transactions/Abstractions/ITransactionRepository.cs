using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<Transaction?> GetByIdAsync(TransactionId id, CancellationToken ct);
    Task<IReadOnlyList<Transaction>> ListAsync(int take, CancellationToken ct);
    Task RemoveAsync(TransactionId id, CancellationToken ct);
    Task<bool> UpdateTwoSplitAsync(
        Guid id,
        DateOnly bookedOn,
        string description,
        Guid? payeeId,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        CancellationToken ct);
    Task<bool> HasAnyAsync(CancellationToken ct);
    
    Task<IReadOnlyList<ExpenseSearchResultDto>> SearchExpensesAsync(
        string query, 
        Guid? expenseAccountId, 
        int limit, 
        CancellationToken ct);
}
