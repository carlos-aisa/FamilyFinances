namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record EconomicStateDto(
    DateOnly AsOf,
    long AssetsTotalCents,
    long LiabilitiesTotalCents,
    long NetWorthCents,
    long IncomeTotalCents,
    long ExpenseTotalCents,
    long PeriodNetResultCents
);
