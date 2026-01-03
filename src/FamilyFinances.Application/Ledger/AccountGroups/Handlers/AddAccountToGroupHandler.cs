using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Application.Ledger.AccountGroups.Requests;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.AccountGroups.Handlers
{
    public sealed class AddAccountToGroupHandler
    {
        private readonly IAccountGroupMembershipRepository _memberships;

        public AddAccountToGroupHandler(IAccountGroupMembershipRepository memberships)
        {
            _memberships = memberships;
        }

        public async Task HandleAsync(AddAccountToGroupRequest request, CancellationToken ct)
        {
            var groupId = new AccountGroupId(request.GroupId);
            var accountId = new AccountId(request.AccountId);

            if (await _memberships.ExistsAsync(groupId, accountId, ct))
                return; // idempotent

            await _memberships.AddAsync(groupId, accountId, ct);
        }
    }
}
