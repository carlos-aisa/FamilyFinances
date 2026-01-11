using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers
{
    public sealed class DeleteTransactionHandler
    {
        readonly ILedgerUnitOfWork _uow;
        readonly ITransactionRepository _transactions;

        public DeleteTransactionHandler(ILedgerUnitOfWork uow, ITransactionRepository transactions)
        {
            _uow = uow;
            _transactions = transactions;
        }

        public async Task<bool> HandleAsync(Guid transactionId, CancellationToken ct)
        {
            var id = new TransactionId(transactionId);
            var tx = await _transactions.GetByIdAsync(id, ct);
            if (tx is null)
                return false;
            await _transactions.RemoveAsync(id, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
