using FamilyFinances.Application.Ledger.Transactions.Abstractions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class HasAnyTransactionHandler
{
    private readonly ITransactionRepository _transactions;

    public HasAnyTransactionHandler(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<bool> HandleAsync(CancellationToken ct)
    {
        return await _transactions.HasAnyAsync(ct);
    }
}
