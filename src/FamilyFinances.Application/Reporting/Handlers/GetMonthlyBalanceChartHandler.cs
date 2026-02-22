using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetMonthlyBalanceChartHandler
{
    private readonly IReportingReadRepository _repo;

    public GetMonthlyBalanceChartHandler(IReportingReadRepository repo)
    {
        _repo = repo;
    }

    public Task<MonthlyBalanceChartDto> HandleAsync(GetMonthlyBalanceChartQuery query, CancellationToken ct)
    {
        ValidateYearAndMonth(query.Year, query.Month);
        ValidateFilters(query.AccountId, query.Nature);
        return _repo.GetMonthlyBalanceChartAsync(query.Year, query.Month, query.AccountId, query.PayeeId, query.Nature, ct);
    }

    private static void ValidateYearAndMonth(int year, int month)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (year < 2000 || year > currentYear)
            throw new DomainException($"Invalid year '{year}'. Expected a value between 2000 and {currentYear}.");

        if (month is < 1 or > 12)
            throw new DomainException("Invalid month. Expected a value between 1 and 12.");
    }

    private static void ValidateFilters(Guid? accountId, AccountNature? nature)
    {
        if (accountId is not null && nature is not null)
            throw new DomainException("Specify either 'accountId' or 'nature' for monthly balance chart, but not both.");
    }
}
