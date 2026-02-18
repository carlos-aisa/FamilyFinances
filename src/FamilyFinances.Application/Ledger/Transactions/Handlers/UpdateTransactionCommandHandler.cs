using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class UpdateTransactionHandler
{
    private readonly ILedgerUnitOfWork _uow;
    private readonly ITransactionRepository _transactions;
    private readonly IFiscalYearGuard _fiscalYearGuard;

    public UpdateTransactionHandler(
        ILedgerUnitOfWork uow,
        ITransactionRepository transactions,
        IFiscalYearGuard fiscalYearGuard)
    {
        _uow = uow;
        _transactions = transactions;
        _fiscalYearGuard = fiscalYearGuard;
    }

    public async Task<bool> HandleAsync(UpdateTransactionRequest request, CancellationToken ct)
    {
        var id = new TransactionId(request.Id);
        var existing = await _transactions.GetByIdAsync(id, ct);
        if (existing is null)
            return false;

        await _fiscalYearGuard.EnsureYearOpenAsync(existing.BookedOn.Year, ct);
        await _fiscalYearGuard.EnsureYearOpenAsync(request.BookedOn.Year, ct);

        return await _transactions.UpdateTwoSplitAsync(
            request.Id,
            request.BookedOn,
            request.Description,
            request.PayeeId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            ct);
    }
}
