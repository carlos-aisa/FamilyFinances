namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetAccountTotalsQuery(
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    bool IncludeZeroAccounts = false
);
