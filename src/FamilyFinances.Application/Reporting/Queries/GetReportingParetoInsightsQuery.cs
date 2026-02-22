using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetReportingParetoInsightsQuery(
    DateOnly From,
    DateOnly To,
    ReportingInsightDimension Dimension,
    int TopN = 5,
    Guid? AccountId = null,
    Guid? PayeeId = null
);
