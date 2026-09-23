using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetAccountGroupMovementsHandler
{
    private readonly IReportingReadRepository _repo;

    public GetAccountGroupMovementsHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<AccountGroupMovementsDto> HandleAsync(GetAccountGroupMovementsQuery query, CancellationToken ct)
    {
        ReportingGuards.EnsureValidPeriod(query.FromInclusive, query.ToExclusive);

        return _repo.GetAccountGroupMovementsAsync(
            groupId: query.GroupId,
            fromInclusive: query.FromInclusive,
            toExclusive: query.ToExclusive,
            nature: query.Nature,
            ct: ct);
    }
}
