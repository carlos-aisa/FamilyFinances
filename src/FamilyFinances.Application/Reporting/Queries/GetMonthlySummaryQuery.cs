namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlySummaryQuery(
    DateOnly From,
    DateOnly To,
    Guid? AccountId = null,
    Guid? PayeeId = null
);
