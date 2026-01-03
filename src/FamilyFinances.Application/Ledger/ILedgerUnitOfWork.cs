namespace FamilyFinances.Application.Ledger;

public interface ILedgerUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
