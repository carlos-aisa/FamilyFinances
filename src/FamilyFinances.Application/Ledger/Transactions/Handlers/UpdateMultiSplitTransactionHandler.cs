using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Requests;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class UpdateMultiSplitTransactionHandler
{
    private readonly ILedgerUnitOfWork _uow;
    private readonly ITransactionRepository _transactions;

    public UpdateMultiSplitTransactionHandler(ILedgerUnitOfWork uow, ITransactionRepository transactions)
    {
        _uow = uow;
        _transactions = transactions;
    }

    public async Task<bool> HandleAsync(UpdateMultiSplitTransactionRequest request, CancellationToken ct)
    {
        return await _transactions.UpdateMultiSplitAsync(
            request.Id,
            request.BookedOn,
            request.Description,
            request.PayeeId,
            request.Splits,
            ct);
    }
}
