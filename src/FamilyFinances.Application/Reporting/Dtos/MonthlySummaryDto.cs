namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    long IncomeTotal,
    long ExpenseTotal,
    long Net,
    int TransactionsCount
);
