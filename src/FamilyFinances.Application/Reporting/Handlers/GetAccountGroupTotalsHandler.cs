using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetAccountGroupTotalsHandler
{
    private readonly IReportingReadRepository _repo;

    public GetAccountGroupTotalsHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<AccountGroupTotalsDto> HandleAsync(GetAccountGroupTotalsQuery query, CancellationToken ct)
    {
        ReportingGuards.EnsureValidPeriod(query.FromInclusive, query.ToExclusive);

        var nature = query.Nature ?? AccountNature.Expense;

        return _repo.GetAccountGroupTotalsAsync(
            groupId: query.GroupId,
            fromInclusive: query.FromInclusive,
            toExclusive: query.ToExclusive,
            nature: nature,
            ct: ct);
    }
}
