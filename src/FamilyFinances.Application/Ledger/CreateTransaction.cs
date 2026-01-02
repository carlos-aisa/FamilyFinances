using FamilyFinances.Application.Abstractions;
using FamilyFinances.Domain.Accounts;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger;

namespace FamilyFinances.Application.Ledger;

public sealed record CreateTransactionCommand(
    DateOnly BookedOn,
    string Description,
    IReadOnlyList<TransactionSplitInput> Splits);

public sealed record TransactionSplitInput(Guid AccountId, long AmountCents, string? Memo);

public sealed class CreateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly ILedgerUnitOfWork _uow;

    public CreateTransactionHandler(ITransactionRepository transactions, ILedgerUnitOfWork uow)
    {
        _transactions = transactions;
        _uow = uow;
    }

    public async Task<TransactionDto> HandleAsync(CreateTransactionCommand cmd, CancellationToken ct)
    {
        var splits = cmd.Splits.Select(s =>
            TransactionSplit.Create(new AccountId(s.AccountId), new Money(s.AmountCents), s.Memo));

        var tx = Transaction.Create(cmd.BookedOn, cmd.Description, splits);

        await _transactions.AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        return new TransactionDto(
            tx.Id.Value,
            tx.BookedOn,
            tx.Description,
            tx.Splits.Select(x => new TransactionSplitDto(x.AccountId.Value, x.Amount.Cents, x.Memo)).ToList()
        );
    }
}
