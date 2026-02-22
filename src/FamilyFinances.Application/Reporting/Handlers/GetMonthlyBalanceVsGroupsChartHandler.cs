using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetMonthlyBalanceVsGroupsChartHandler
{
    private readonly IReportingReadRepository _repo;

    public GetMonthlyBalanceVsGroupsChartHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<MonthlyBalanceVsGroupsChartDto> HandleAsync(GetMonthlyBalanceVsGroupsChartQuery query, CancellationToken ct)
    {
        ValidateYearAndMonth(query.Year, query.Month);
        return _repo.GetMonthlyBalanceVsGroupsChartAsync(query.Year, query.Month, ct);
    }

    private static void ValidateYearAndMonth(int year, int month)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (year < 2000 || year > currentYear)
            throw new DomainException($"Invalid year '{year}'. Expected a value between 2000 and {currentYear}.");

        if (month is < 1 or > 12)
            throw new DomainException("Invalid month. Expected a value between 1 and 12.");
    }
}
