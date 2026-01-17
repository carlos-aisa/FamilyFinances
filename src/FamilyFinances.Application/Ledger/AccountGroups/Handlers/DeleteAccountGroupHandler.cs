using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers;

public sealed class DeleteAccountGroupHandler
{
    private readonly IAccountGroupRepository _groups;
    private readonly ILedgerUnitOfWork _uow;

    public DeleteAccountGroupHandler(IAccountGroupRepository groups, ILedgerUnitOfWork uow)
    {
        _groups = groups;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid groupId, CancellationToken ct)
    {
        var id = new AccountGroupId(groupId);
        var group = await _groups.GetByIdAsync(id, ct);

        if (group is null)
            return false;

        _groups.Remove(group);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
