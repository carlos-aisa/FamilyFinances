using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class DeleteAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public DeleteAccountHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid accountId, CancellationToken ct)
    {
        var id = new AccountId(accountId);

        var account = await _accounts.GetByIdForUpdateAsync(id, ct);
        if (account is null)
            return false;

        var referenced = await _accounts.IsReferencedBySplitsAsync(id, ct);
        if (referenced)
            throw new ConflictException("Account cannot be deleted because it is referenced by transactions. Close it instead.");

        _accounts.Remove(account);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
