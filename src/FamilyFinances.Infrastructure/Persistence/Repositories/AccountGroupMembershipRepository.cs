using FamilyFinances.Application.Ledger.AccountGroups.Abstractions;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class AccountGroupMembershipRepository : IAccountGroupMembershipRepository
{
    private readonly LedgerDbContext _db;

    public AccountGroupMembershipRepository(LedgerDbContext db) => _db = db;

    public Task<bool> ExistsAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct)
        => _db.AccountGroupMembers
            .AsNoTracking()
            .AnyAsync(x => x.GroupId == groupId && x.AccountId == accountId, ct);

    public Task AddAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct)
        => _db.AccountGroupMembers
            .AddAsync(new AccountGroupMember(groupId, accountId), ct)
            .AsTask();

    public async Task RemoveAsync(AccountGroupId groupId, AccountId accountId, CancellationToken ct)
    {
        var entity = await _db.AccountGroupMembers
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.AccountId == accountId, ct);

        if (entity is null)
            return; // idempotent

        _db.AccountGroupMembers.Remove(entity);
    }

    public async Task<IReadOnlyList<Account>> ListAccountsForGroupAsync(AccountGroupId groupId, CancellationToken ct)
    {
        var accountIds = await _db.AccountGroupMembers
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .Select(x => x.AccountId)
            .ToListAsync(ct);

        if (accountIds.Count == 0)
            return Array.Empty<Account>();

        return await _db.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AccountGroup>> ListGroupsForAccountAsync(AccountId accountId, CancellationToken ct)
    {
        var groupIds = await _db.AccountGroupMembers
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => x.GroupId)
            .ToListAsync(ct);

        if (groupIds.Count == 0)
            return Array.Empty<AccountGroup>();

        return await _db.AccountGroups
            .AsNoTracking()
            .Where(g => groupIds.Contains(g.Id))
            .OrderBy(g => g.Name)
            .ToListAsync(ct);
    }
}
