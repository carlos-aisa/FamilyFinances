using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Dtos;

public enum ReportingInsightDimension
{
    Group = 1,
    Payee = 2
}

public sealed record InsightContributorAggregateDto(
    Guid? EntityId,
    string DisplayName,
    long AmountCents
);

public sealed record InsightMonthlyContributorAggregateDto(
    Guid? EntityId,
    string DisplayName,
    int Year,
    int Month,
    long AmountCents
);

public sealed record ParetoContributorDto(
    Guid? EntityId,
    string DisplayName,
    long AmountCents,
    decimal ContributionPercentage
);

public sealed record ParetoInsightSectionDto(
    AccountNature Nature,
    long TotalAmountCents,
    int TopN,
    long TopNAmountCents,
    decimal TopNCoveragePercentage,
    IReadOnlyList<ParetoContributorDto> Contributors
);

public sealed record ReportingParetoInsightsDto(
    DateOnly From,
    DateOnly To,
    ReportingInsightDimension Dimension,
    ParetoInsightSectionDto Expense,
    ParetoInsightSectionDto Income
);

public sealed record AnomalyContributorDto(
    Guid? EntityId,
    string DisplayName,
    long CurrentAmountCents,
    long BaselineMeanAmountCents,
    long ThresholdAmountCents,
    decimal? ZScore,
    bool IsAnomaly,
    bool IsInsufficientHistory,
    int HistoryMonthsCount,
    string Explanation
);

public sealed record ReportingAnomalyInsightsDto(
    int Year,
    int Month,
    AccountNature Nature,
    ReportingInsightDimension Dimension,
    int RequiredHistoryMonths,
    string ThresholdRule,
    IReadOnlyList<AnomalyContributorDto> Contributors
);
