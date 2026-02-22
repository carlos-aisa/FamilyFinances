using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetReportingAnomalyInsightsQuery(
    int Year,
    int Month,
    AccountNature Nature,
    ReportingInsightDimension Dimension,
    int LookbackMonths = 12,
    int RequiredHistoryMonths = 3,
    Guid? AccountId = null,
    Guid? PayeeId = null
);
