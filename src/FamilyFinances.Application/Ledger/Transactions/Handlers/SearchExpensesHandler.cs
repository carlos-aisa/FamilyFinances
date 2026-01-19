using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class SearchExpensesHandler
{
    private readonly ITransactionRepository _transactions;

    public SearchExpensesHandler(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<ExpenseSearchResultDto>> HandleAsync(
        string query, 
        Guid? expenseAccountId, 
        int limit, 
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Array.Empty<ExpenseSearchResultDto>();

        return await _transactions.SearchExpensesAsync(
            query.Trim(), 
            expenseAccountId, 
            Math.Min(limit, 50), // Cap at 50
            ct);
    }
}