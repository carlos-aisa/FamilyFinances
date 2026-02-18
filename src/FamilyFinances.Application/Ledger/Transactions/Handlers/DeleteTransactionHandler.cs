using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class DeleteTransactionHandler
{
    private readonly ILedgerUnitOfWork _uow;
    private readonly ITransactionRepository _transactions;
    private readonly IFiscalYearGuard _fiscalYearGuard;

    public DeleteTransactionHandler(
        ILedgerUnitOfWork uow,
        ITransactionRepository transactions,
        IFiscalYearGuard fiscalYearGuard)
    {
        _uow = uow;
        _transactions = transactions;
        _fiscalYearGuard = fiscalYearGuard;
    }

    public async Task<bool> HandleAsync(Guid transactionId, CancellationToken ct)
    {
        var id = new TransactionId(transactionId);
        var tx = await _transactions.GetByIdAsync(id, ct);
        if (tx is null)
            return false;

        await _fiscalYearGuard.EnsureYearOpenAsync(tx.BookedOn.Year, ct);

        await _transactions.RemoveAsync(id, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
