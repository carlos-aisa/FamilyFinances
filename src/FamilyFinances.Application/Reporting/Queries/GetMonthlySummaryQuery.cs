namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlySummaryQuery(
    int Year,
    int Month,
    Guid? AccountId = null,
    Guid? PayeeId = null
);
