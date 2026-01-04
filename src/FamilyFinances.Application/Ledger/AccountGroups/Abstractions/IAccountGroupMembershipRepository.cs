using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.AccountGroups.Abstractions;

public interface IAccountGroupMembershipRepository
{
    Task<bool> ExistsAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct);
    Task AddAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct);
    Task RemoveAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct);
    Task<IReadOnlyList<Account>> ListAccountsForGroupAsync(AccountGroupId groupId, CancellationToken ct);
}