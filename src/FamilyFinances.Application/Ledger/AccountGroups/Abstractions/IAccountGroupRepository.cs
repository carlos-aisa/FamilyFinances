using FamilyFinances.Domain.Ledger.AccountGroups;

namespace FamilyFinances.Application.Ledger.AccountGroups.Abstractions;

public interface IAccountGroupRepository
{
    Task AddAsync(AccountGroup group, CancellationToken ct);
    Task<AccountGroup?> GetByIdAsync(AccountGroupId id, CancellationToken ct);
    Task<AccountGroup?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct);
    Task<IReadOnlyList<AccountGroup>> ListAsync(CancellationToken ct);
    void Remove(AccountGroup group);
}
