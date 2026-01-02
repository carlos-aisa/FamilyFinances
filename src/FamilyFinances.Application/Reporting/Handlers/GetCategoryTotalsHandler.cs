using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetCategoryTotalsHandler
{
    private readonly IReportingReadRepository _repo;

    public GetCategoryTotalsHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<CategoryTotalsDto> HandleAsync(GetCategoryTotalsQuery query, CancellationToken ct)
    {
        ReportingGuards.EnsureValidPeriod(query.FromInclusive, query.ToExclusive);

        return _repo.GetCategoryTotalsAsync(
            query.FromInclusive,
            query.ToExclusive,
            query.Nature,
            query.PayeeId,
            ct);
    }
}
