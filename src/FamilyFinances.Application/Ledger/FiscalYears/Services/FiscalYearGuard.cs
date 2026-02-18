using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Application.Ledger.FiscalYears.Services;

public sealed class FiscalYearGuard : IFiscalYearGuard
{
    private readonly IFiscalYearGovernanceRepository _governance;

    public FiscalYearGuard(IFiscalYearGovernanceRepository governance)
    {
        _governance = governance;
    }

    public Task<bool> IsYearClosedAsync(int year, CancellationToken ct)
    {
        return _governance.IsYearClosedAsync(year, ct);
    }

    public async Task EnsureYearOpenAsync(int year, CancellationToken ct)
    {
        if (await _governance.IsYearClosedAsync(year, ct))
            throw new DomainException($"Year {year} is closed. Reopen the year to modify movements.");
    }
}
