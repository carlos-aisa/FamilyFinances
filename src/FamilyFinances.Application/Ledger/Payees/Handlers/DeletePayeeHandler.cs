using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Handlers;

public sealed class DeletePayeeHandler
{
    private readonly IPayeeRepository _payees;
    private readonly ILedgerUnitOfWork _uow;

    public DeletePayeeHandler(IPayeeRepository payees, ILedgerUnitOfWork uow)
    {
        _payees = payees;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid payeeId, CancellationToken ct)
    {
        var id = new PayeeId(payeeId);

        var payee = await _payees.GetByIdForUpdateAsync(id, ct);
        if (payee is null)
            return false;

        var referenced = await _payees.IsReferencedByTransactionsAsync(id, ct);
        if (referenced)
            throw new ConflictException("Payee cannot be deleted because it is referenced by transactions.");

        _payees.Remove(payee);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
