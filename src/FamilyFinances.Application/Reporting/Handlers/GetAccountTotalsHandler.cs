using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetAccountTotalsHandler
{
    private readonly IReportingReadRepository _repo;

    public GetAccountTotalsHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<AccountTotalsDto> HandleAsync(GetAccountTotalsQuery query, CancellationToken ct)
    {
        ReportingGuards.EnsureValidPeriod(query.FromInclusive, query.ToExclusive);

        return _repo.GetAccountTotalsAsync(
            query.FromInclusive,
            query.ToExclusive,
            query.IncludeZeroAccounts,
            ct);
    }
}
