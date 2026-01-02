using FamilyFinances.Application.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Create;

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
