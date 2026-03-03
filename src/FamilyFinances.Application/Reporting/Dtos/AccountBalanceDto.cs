namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AccountBalanceDto(
    Guid AccountId,
    decimal Balance,
    decimal CurrentMonthBalance
);
