namespace FamilyFinances.Application.Abstractions;

public interface ILedgerUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
