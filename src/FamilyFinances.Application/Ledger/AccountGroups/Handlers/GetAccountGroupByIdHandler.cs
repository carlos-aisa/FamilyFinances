using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Dtos;
using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers
{
    public sealed class GetAccountGroupByIdHandler
    {
        private readonly IAccountGroupRepository _groups;
        private readonly IAccountGroupMembershipRepository _memberships;

        public GetAccountGroupByIdHandler(
            IAccountGroupRepository groups,
            IAccountGroupMembershipRepository memberships)
        {
            _groups = groups;
            _memberships = memberships;
        }

        public async Task<AccountGroupDetailsDto> HandleAsync(Guid groupId, CancellationToken ct)
        {
            var id = new AccountGroupId(groupId);

            var group = await _groups.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Account group not found.");

            var accounts = await _memberships.ListAccountsForGroupAsync(id, ct);

            return new AccountGroupDetailsDto(
                group.Id.Value,
                group.Name,
                group.Description,
                accounts.Select(a =>
                    new AccountRefDto(
                        a.Id.Value,
                        a.Name,
                        a.Nature,
                    a.Kind))
                .ToList(),
                group.IsDashboardPinned);
        }
    }
}
