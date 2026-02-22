using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetReportingAnomalyInsightsHandler
{
    private readonly IReportingReadRepository _repository;
    private readonly IReportingInsightsCalculator _calculator;

    public GetReportingAnomalyInsightsHandler(
        IReportingReadRepository repository,
        IReportingInsightsCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task<ReportingAnomalyInsightsDto> HandleAsync(
        GetReportingAnomalyInsightsQuery query,
        CancellationToken ct)
    {
        Validate(query);

        var targetMonth = new DateOnly(query.Year, query.Month, 1);
        var fromInclusive = targetMonth.AddMonths(-query.LookbackMonths);
        var toExclusive = targetMonth.AddMonths(1);

        var monthlyRows = await _repository.GetMonthlyInsightContributorTotalsAsync(
            fromInclusive,
            toExclusive,
            query.Nature,
            query.Dimension,
            query.AccountId,
            query.PayeeId,
            ct);

        return _calculator.BuildMonthlyAnomalyInsights(
            query.Year,
            query.Month,
            query.Nature,
            query.Dimension,
            query.LookbackMonths,
            query.RequiredHistoryMonths,
            monthlyRows);
    }

    private static void Validate(GetReportingAnomalyInsightsQuery query)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (query.Year < 2000 || query.Year > currentYear)
            throw new DomainException($"Invalid year '{query.Year}'. Expected a value between 2000 and {currentYear}.");

        if (query.Month is < 1 or > 12)
            throw new DomainException("Invalid month. Expected a value between 1 and 12.");

        if (query.Nature is not AccountNature.Expense and not AccountNature.Income)
            throw new DomainException("Anomaly insights support only 'Expense' or 'Income' nature.");

        if (query.LookbackMonths is < 3 or > 36)
            throw new DomainException("Invalid lookbackMonths. Expected a value between 3 and 36.");

        if (query.RequiredHistoryMonths is < 2 or > 12)
            throw new DomainException("Invalid requiredHistoryMonths. Expected a value between 2 and 12.");

        if (query.RequiredHistoryMonths > query.LookbackMonths)
            throw new DomainException("requiredHistoryMonths cannot be greater than lookbackMonths.");

        if (query.Dimension == ReportingInsightDimension.Payee && query.PayeeId is not null)
            throw new DomainException("Filtering by payee is not supported when dimension is 'payee'.");
    }
}
