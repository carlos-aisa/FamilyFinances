using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers
{
    public sealed class UpdateTransactionHandler
    {
        private readonly ILedgerUnitOfWork _uow;
        private readonly ITransactionRepository _transactions;

        public UpdateTransactionHandler(ILedgerUnitOfWork uow, ITransactionRepository transactions)
        {
            _uow = uow;
            _transactions = transactions;
        }

        public async Task<bool> HandleAsync(UpdateTransactionRequest request, CancellationToken ct)
        {
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
}
