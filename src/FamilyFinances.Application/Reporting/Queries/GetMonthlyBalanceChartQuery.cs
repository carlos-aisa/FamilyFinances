using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlyBalanceChartQuery(
    int Year,
    int Month,
    Guid? AccountId = null,
    Guid? PayeeId = null,
    AccountNature? Nature = null
);
