using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;

public sealed class CreateTransactionHandler
{
    private readonly ITransactionRepository _transactions;
    private readonly IPayeeRepository _payees;
    private readonly ITransactionLinkRepository _links;
    private readonly ILedgerUnitOfWork _uow;

    public CreateTransactionHandler(
        ITransactionRepository transactions, 
        IPayeeRepository payees,
        ITransactionLinkRepository links,
        ILedgerUnitOfWork uow)
    {
        _transactions = transactions;
        _payees = payees;
        _links = links;
        _uow = uow;
    }

    public async Task<TransactionDto> HandleAsync(CreateTransactionRequest cmd, CancellationToken ct)
    {
        PayeeId? payeeId = null;

        if (cmd.PayeeId.HasValue)
        {
            if (cmd.PayeeId.Value == Guid.Empty)
                throw new DomainException("PayeeId cannot be empty.");

            payeeId = new PayeeId(cmd.PayeeId.Value);

            var exists = await _payees.GetByIdAsync(payeeId.Value, ct);
            if (exists is null)
                throw new DomainException($"Payee '{cmd.PayeeId}' not found.");
        }

        // Validate RelatedTransactionId if provided
        TransactionId? relatedTransactionId = null;
        if (cmd.RelatedTransactionId.HasValue)
        {
            if (cmd.RelatedTransactionId.Value == Guid.Empty)
                throw new DomainException("RelatedTransactionId cannot be empty.");

            relatedTransactionId = new TransactionId(cmd.RelatedTransactionId.Value);

            var relatedTx = await _transactions.GetByIdAsync(relatedTransactionId.Value, ct);
            if (relatedTx is null)
                throw new DomainException($"Related transaction '{cmd.RelatedTransactionId}' not found.");
        }

        var splits = cmd.Splits.Select(s =>
            TransactionSplit.Create(new AccountId(s.AccountId), new Money(s.AmountCents), s.Memo));

        var tx = Transaction.Create(cmd.BookedOn, cmd.Description, splits, payeeId);

        await _transactions.AddAsync(tx, ct);

        // If related transaction is provided, create a link
        if (relatedTransactionId is not null)
        {
            var link = TransactionLink.Create(
                sourceTransactionId: relatedTransactionId.Value,
                targetTransactionId: tx.Id,
                type: TransactionLinkType.Refund,
                linkedOn: cmd.BookedOn);

            await _links.AddAsync(link, ct);
        }

        await _uow.SaveChangesAsync(ct);

        return new TransactionDto(
            tx.Id.Value,
            tx.BookedOn,
            tx.Description,
            tx.PayeeId?.Value,
            tx.Payee?.Name,
            tx.Splits.Select(x => new TransactionSplitDto(x.AccountId.Value, x.Amount.ToEuros(), x.Memo)).ToList()
        );
    }
}
