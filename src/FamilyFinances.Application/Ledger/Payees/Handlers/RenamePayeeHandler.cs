using FamilyFinances.Application.Ledger.Payees.Abstractions;
using FamilyFinances.Application.Ledger.Payees.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Handlers;

public sealed class RenamePayeeHandler
{
    private readonly IPayeeRepository _payees;
    private readonly ILedgerUnitOfWork _uow;

    public RenamePayeeHandler(IPayeeRepository payees, ILedgerUnitOfWork uow)
    {
        _payees = payees;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid payeeId, RenamePayeeRequest request, CancellationToken ct)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var id = new PayeeId(payeeId);

        var payee = await _payees.GetByIdAsync(id, ct);
        if (payee is null)
            return false;

        var normalized = NameNormalizer.Normalize(request.Name);

        var existing = await _payees.GetByNormalizedNameAsync(normalized, ct);
        if (existing is not null && existing.Id.Value != payeeId)
            throw new ConflictException($"Payee '{request.Name}' already exists.");

        payee.Rename(request.Name);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
