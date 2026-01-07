using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class ListTransactionsHandler
{
    private readonly ITransactionRepository _repo;

    public ListTransactionsHandler(ITransactionRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<TransactionDto>> HandleAsync(int take, CancellationToken ct)
    {
        var items = await _repo.ListAsync(take, ct);

        return items.Select(t => new TransactionDto(
                t.Id.Value,
                t.BookedOn,
                t.Description,
                t.PayeeId?.Value,
                t.Splits.Select(s => new TransactionSplitDto(
                    s.AccountId.Value,
                    s.Amount.Cents,
                    s.Memo)).ToList()))
            .ToList();
    }
}
