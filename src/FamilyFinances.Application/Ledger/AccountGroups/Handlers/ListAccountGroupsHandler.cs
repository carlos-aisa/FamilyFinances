using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers
{
    public sealed class ListAccountGroupsHandler
    {
        private readonly IAccountGroupRepository _groups;

        public ListAccountGroupsHandler(IAccountGroupRepository groups)
        {
            _groups = groups;
        }

        public async Task<IReadOnlyList<AccountGroupDto>> HandleAsync(CancellationToken ct)
        {
            var groups = await _groups.ListAsync(ct);

            return groups
                .Select(g => new AccountGroupDto(g.Id.Value, g.Name, g.Description, g.IsDashboardPinned))
                .ToList();
        }
    }
}
