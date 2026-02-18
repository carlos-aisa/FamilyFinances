namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AssetTotalBalanceDto(
    DateOnly AsOf,
    long TotalCents,
    int AssetAccountsCount
);
