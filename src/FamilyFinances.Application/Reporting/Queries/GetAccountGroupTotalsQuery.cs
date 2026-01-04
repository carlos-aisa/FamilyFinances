using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetAccountGroupTotalsQuery(
    Guid GroupId,
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    AccountNature? Nature = null
);
