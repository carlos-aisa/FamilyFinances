using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;

namespace FamilyFinances.Application.Ledger.FiscalYears.Handlers;

public sealed class ListFiscalYearsHandler
{
    private readonly IFiscalYearGovernanceRepository _governance;

    public ListFiscalYearsHandler(IFiscalYearGovernanceRepository governance)
    {
        _governance = governance;
    }

    public Task<IReadOnlyList<FiscalYearStatusDto>> HandleAsync(CancellationToken ct)
    {
        return _governance.ListStatusesAsync(ct);
    }
}
