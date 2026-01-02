using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetMonthlySummaryHandler
{
    private readonly IReportingReadRepository _repo;

    public GetMonthlySummaryHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<MonthlySummaryDto> HandleAsync(GetMonthlySummaryQuery query, CancellationToken ct)
    {
        ReportingGuards.EnsureValidYear(query.Year);
        ReportingGuards.EnsureValidMonth(query.Month);

        return _repo.GetMonthlySummaryAsync(
            query.Year,
            query.Month,
            query.AccountId,
            query.PayeeId,
            ct);
    }
}
