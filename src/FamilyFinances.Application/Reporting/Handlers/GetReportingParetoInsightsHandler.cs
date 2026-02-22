using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Reporting.Queries;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Handlers;

public sealed class GetReportingParetoInsightsHandler
{
    private readonly IReportingReadRepository _repository;
    private readonly IReportingInsightsCalculator _calculator;

    public GetReportingParetoInsightsHandler(
        IReportingReadRepository repository,
        IReportingInsightsCalculator calculator)
    {
        _repository = repository;
        _calculator = calculator;
    }

    public async Task<ReportingParetoInsightsDto> HandleAsync(
        GetReportingParetoInsightsQuery query,
        CancellationToken ct)
    {
        Validate(query);

        var expenseRows = await _repository.GetInsightContributorTotalsAsync(
            query.From,
            query.To,
            AccountNature.Expense,
            query.Dimension,
            query.AccountId,
            query.PayeeId,
            ct);

        var incomeRows = await _repository.GetInsightContributorTotalsAsync(
            query.From,
            query.To,
            AccountNature.Income,
            query.Dimension,
            query.AccountId,
            query.PayeeId,
            ct);

        return _calculator.BuildParetoInsights(
            query.From,
            query.To,
            query.Dimension,
            query.TopN,
            expenseRows,
            incomeRows);
    }

    private static void Validate(GetReportingParetoInsightsQuery query)
    {
        ReportingGuards.EnsureValidPeriod(query.From, query.To);

        if (query.TopN is < 1 or > 20)
            throw new DomainException("Invalid topN. Expected a value between 1 and 20.");

        if (query.Dimension == ReportingInsightDimension.Payee && query.PayeeId is not null)
            throw new DomainException("Filtering by payee is not supported when dimension is 'payee'.");
    }
}
