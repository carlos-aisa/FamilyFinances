using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Ledger.Payees.Abstractions;

public interface IPayeeRepository
{
    Task AddAsync(Payee payee, CancellationToken ct);
    Task<IReadOnlyList<Payee>> ListAsync(CancellationToken ct);
    Task<Payee?> GetByIdAsync(PayeeId id, CancellationToken ct);
    Task<Payee?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct);
    Task<Payee?> GetByIdForUpdateAsync(PayeeId id, CancellationToken ct);
    void Remove(Payee payee);
    Task<bool> IsReferencedByTransactionsAsync(PayeeId id, CancellationToken ct);

}
