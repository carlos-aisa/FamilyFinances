using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class DeleteAccountKindHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public DeleteAccountKindHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid kindId, CancellationToken ct)
    {
        var id = new AccountKindCatalogId(kindId);
        var kind = await _accounts.GetKindByIdAsync(id, ct);
        if (kind is null)
            return false;

        if (kind.IsSystem)
            throw new DomainException("System account kinds cannot be deleted.");

        if (await _accounts.IsKindReferencedByAccountsAsync(id, ct))
            throw new DomainException("Account kind cannot be deleted because it is in use.");

        _accounts.RemoveKind(kind);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
