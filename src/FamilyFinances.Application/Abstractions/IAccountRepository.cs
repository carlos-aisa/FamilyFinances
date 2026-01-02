using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct);
    Task<IReadOnlyList<Account>> ListAsync(CancellationToken ct);
    Task<Account?> GetByIdAsync(AccountId id, CancellationToken ct);
}
