using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;

namespace FamilyFinances.Application.Ledger.FiscalYears.Handlers;

public sealed class GetHistoricalAccountMovementsHandler
{
    private readonly IReportingReadRepository _reporting;

    public GetHistoricalAccountMovementsHandler(IReportingReadRepository reporting)
    {
        _reporting = reporting;
    }

    public async Task<AccountMovementsDto> HandleAsync(
        GetHistoricalAccountMovementsRequest request,
        CancellationToken ct)
    {
        var fromInclusive = new DateOnly(request.Year, 1, 1);
        var toExclusive = new DateOnly(request.Year + 1, 1, 1);
        var safePage = request.Page < 1 ? 1 : request.Page;
        var safePageSize = request.PageSize < 1 ? 50 : Math.Min(request.PageSize, 100);
        var skip = (safePage - 1) * safePageSize;

        return await _reporting.GetAccountMovementsAsync(
            request.AccountId,
            fromInclusive,
            toExclusive,
            request.SearchQuery,
            skip,
            safePageSize,
            ct);
    }
}
