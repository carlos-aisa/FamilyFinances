using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class SetAccountKindHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public SetAccountKindHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid accountId, SetAccountKindRequest request, CancellationToken ct)
    {
        var id = new AccountId(accountId);
        var account = await _accounts.GetByIdForUpdateAsync(id, ct);
        if (account is null)
            return false;

        var selectedKind = await _accounts.GetKindByIdAsync(new AccountKindCatalogId(request.KindId), ct)
            ?? throw new DomainException("Selected account kind does not exist.");

        if (!selectedKind.IsActive)
            throw new DomainException("Selected account kind is inactive.");

        if (!AccountKindCatalogDefaults.IsCompatible(account.Nature, selectedKind))
            throw new DomainException("Selected account kind is not compatible with account nature.");

        account.AssignKind(selectedKind.Id);
        account.SetLegacyKind(selectedKind.LegacyKind);

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
