using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetAssetTotalBalanceHandler
{
    private readonly IReportingReadRepository _repo;

    public GetAssetTotalBalanceHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<AssetTotalBalanceDto> HandleAsync(GetAssetTotalBalanceQuery query, CancellationToken ct)
    {
        return _repo.GetAssetTotalBalanceAsync(query.AsOf, ct);
    }
}
