using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers;

public sealed class RenameAccountGroupHandler
{
    private readonly IAccountGroupRepository _groups;
    private readonly ILedgerUnitOfWork _uow;

    public RenameAccountGroupHandler(IAccountGroupRepository groups, ILedgerUnitOfWork uow)
    {
        _groups = groups;
        _uow = uow;
    }

    public async Task<bool> HandleAsync(Guid groupId, RenameAccountGroupRequest request, CancellationToken ct)
    {
        var id = new AccountGroupId(groupId);
        var group = await _groups.GetByIdAsync(id, ct);

        if (group is null)
            return false;

        var newName = request.Name.Trim();
        var normalized = NameNormalizer.Normalize(newName);

        // Check for duplicate name (excluding current group)
        var existing = await _groups.GetByNormalizedNameAsync(normalized, ct);
        if (existing is not null && existing.Id != id)
            throw new DomainException("An account group with the same name already exists.");

        group.Rename(newName);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
