using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Abstractions;

public interface IReportingInsightsCalculator
{
    ReportingParetoInsightsDto BuildParetoInsights(
        DateOnly from,
        DateOnly to,
        ReportingInsightDimension dimension,
        int topN,
        IReadOnlyList<InsightContributorAggregateDto> expenseContributors,
        IReadOnlyList<InsightContributorAggregateDto> incomeContributors);

    ReportingAnomalyInsightsDto BuildMonthlyAnomalyInsights(
        int year,
        int month,
        AccountNature nature,
        ReportingInsightDimension dimension,
        int lookbackMonths,
        int requiredHistoryMonths,
        IReadOnlyList<InsightMonthlyContributorAggregateDto> monthlyContributors);
}
