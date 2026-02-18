using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;

namespace FamilyFinances.Application.Ledger.FiscalYears.Handlers;

public sealed class ReopenFiscalYearHandler
{
    private readonly IFiscalYearGovernanceRepository _governance;
    private readonly ILedgerUnitOfWork _uow;

    public ReopenFiscalYearHandler(IFiscalYearGovernanceRepository governance, ILedgerUnitOfWork uow)
    {
        _governance = governance;
        _uow = uow;
    }

    public async Task<FiscalYearStatusDto> HandleAsync(ReopenFiscalYearRequest request, CancellationToken ct)
    {
        await _governance.ReopenYearAsync(request.Year, request.ActorUserId, ct);
        await _uow.SaveChangesAsync(ct);

        var statuses = await _governance.ListStatusesAsync(ct);
        return statuses.First(s => s.Year == request.Year);
    }
}
