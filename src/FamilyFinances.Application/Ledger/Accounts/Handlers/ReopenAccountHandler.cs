using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class ReopenAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public ReopenAccountHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid accountId, CancellationToken ct)
    {
        var id = new AccountId(accountId);

        var account = await _accounts.GetByIdAsync(id, ct);
        if (account is null)
            return false;

        var exists = await _accounts.ExistsByNormalizedNameAsync(account.NormalizedName, account.Id, ct);
        if (exists)
            throw new ConflictException("An active account with the same name already exists. Rename it before reopening.");

        account.Reopen();
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
