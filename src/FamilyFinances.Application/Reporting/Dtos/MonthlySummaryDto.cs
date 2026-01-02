namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    decimal IncomeTotal,
    decimal ExpenseTotal,
    decimal Net,
    int TransactionsCount
);
