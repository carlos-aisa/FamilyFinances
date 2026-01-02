using FamilyFinances.Domain.Ledger.Payees;

namespace FamilyFinances.Application.Abstractions;

public interface IPayeeRepository
{
    Task<Payee?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct);
    Task<IReadOnlyList<Payee>> ListAsync(CancellationToken ct);
    Task AddAsync(Payee payee, CancellationToken ct);
}
