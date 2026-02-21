using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetEconomicStateHandler
{
    private readonly IReportingReadRepository _repo;

    public GetEconomicStateHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<EconomicStateDto> HandleAsync(GetEconomicStateQuery query, CancellationToken ct)
    {
        var periodFromInclusive = new DateOnly(query.AsOf.Year, query.AsOf.Month, 1);
        var periodToExclusive = query.AsOf.AddDays(1);

        return _repo.GetEconomicStateAsync(
            query.AsOf,
            periodFromInclusive,
            periodToExclusive,
            ct);
    }
}
