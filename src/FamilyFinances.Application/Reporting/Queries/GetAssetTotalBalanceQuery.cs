namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetAssetTotalBalanceQuery(
    DateOnly AsOf
);
