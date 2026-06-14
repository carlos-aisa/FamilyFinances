using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class SetAccountKindActiveHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public SetAccountKindActiveHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task HandleAsync(Guid kindId, SetAccountKindActiveRequest request, CancellationToken ct)
    {
        var id = new AccountKindCatalogId(kindId);
        var kind = await _accounts.GetKindByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Account kind not found.");

        if (kind.IsSystem && !request.IsActive)
            throw new DomainException("System account kinds cannot be deactivated.");

        if (request.IsActive)
            kind.Activate();
        else
            kind.Deactivate();

        await _uow.SaveChangesAsync(ct);
    }
}
