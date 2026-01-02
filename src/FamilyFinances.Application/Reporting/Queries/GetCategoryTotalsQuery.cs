using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetCategoryTotalsQuery(
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    AccountNature Nature,
    Guid? PayeeId = null
);
