using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class GetTransactionByIdHandler
{
    private readonly ITransactionRepository _transactions;

    public GetTransactionByIdHandler(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<TransactionDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var tx = await _transactions.GetByIdAsync(new TransactionId(id), ct);
        if (tx is null) return null;

        return new TransactionDto(
            tx.Id.Value,
            tx.BookedOn,
            tx.Description,
            tx.PayeeId?.Value,
            tx.Splits.Select(x => new TransactionSplitDto(x.AccountId.Value, x.Amount.Cents, x.Memo)).ToList()
        );
    }
}
