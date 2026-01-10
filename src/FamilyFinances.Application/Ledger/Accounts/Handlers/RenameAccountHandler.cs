using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

public sealed class RenameAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerUnitOfWork _uow;

    public RenameAccountHandler(IAccountRepository accounts, ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid accountId, RenameAccountRequest request, CancellationToken ct)
    {
        var id = new AccountId(accountId);

        var account = await _accounts.GetByIdAsync(id, ct);
        if (account is null)
            return false;

        var newName = (request.Name ?? string.Empty).Trim();
        var normalized = NameNormalizer.Normalize(newName);

        var exists = await _accounts.ExistsByNormalizedNameAsync(normalized, id, ct);
        if (exists)
            throw new ConflictException("Account name already exists.");

        account.Rename(newName);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
