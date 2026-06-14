using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct);
    Task<IReadOnlyList<Account>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<AccountKindCatalog>> ListKindsAsync(bool includeInactive, CancellationToken ct);
    Task AddKindAsync(AccountKindCatalog kind, CancellationToken ct);
    Task<AccountKindCatalog?> GetKindByIdAsync(AccountKindCatalogId id, CancellationToken ct);
    Task<AccountKindCatalog?> GetKindByKeyAsync(string key, CancellationToken ct);
    Task<AccountKindCatalog?> GetKindByLegacyAsync(AccountKind legacyKind, CancellationToken ct);
    Task<AccountKindCatalog?> GetKindByLegacyAndNatureAsync(AccountKind legacyKind, AccountNature nature, CancellationToken ct);
    Task<bool> IsKindReferencedByAccountsAsync(AccountKindCatalogId id, CancellationToken ct);
    Task<Account?> GetByIdAsync(AccountId id, CancellationToken ct);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, AccountId? excludingId, CancellationToken ct);
    Task<bool> ExistsKindByKeyAsync(string key, AccountKindCatalogId? excludingId, CancellationToken ct);
    Task<Account?> GetByIdForUpdateAsync(AccountId id, CancellationToken ct);
    Task<bool> IsReferencedBySplitsAsync(AccountId id, CancellationToken ct);
    void Remove(Account account);
    void RemoveKind(AccountKindCatalog kind);
}
