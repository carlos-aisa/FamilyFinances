namespace FamilyFinances.Application.Ledger.FiscalYears.Abstractions;

public interface IFiscalYearGuard
{
    Task<bool> IsYearClosedAsync(int year, CancellationToken ct);
    Task EnsureYearOpenAsync(int year, CancellationToken ct);
}
