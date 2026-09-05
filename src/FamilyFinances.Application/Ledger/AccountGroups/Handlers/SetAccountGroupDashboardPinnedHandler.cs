using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers;

public sealed class SetAccountGroupDashboardPinnedHandler
{
    private readonly IAccountGroupRepository _groups;
    private readonly ILedgerUnitOfWork _uow;

    public SetAccountGroupDashboardPinnedHandler(IAccountGroupRepository groups, ILedgerUnitOfWork uow)
    {
        _groups = groups;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid groupId, SetAccountGroupDashboardPinnedRequest request, CancellationToken ct)
    {
        var group = await _groups.GetByIdAsync(new AccountGroupId(groupId), ct);
        if (group is null)
            return false;

        group.SetDashboardPinned(request.IsDashboardPinned);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
