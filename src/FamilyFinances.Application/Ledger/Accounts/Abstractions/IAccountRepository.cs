using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct);
    Task<IReadOnlyList<Account>> ListAsync(CancellationToken ct);
    Task<Account?> GetByIdAsync(AccountId id, CancellationToken ct);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, AccountId? excludingId, CancellationToken ct);

}
