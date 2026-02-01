namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record MonthlySummaryDto(
    DateOnly From,
    DateOnly To,
    long IncomeTotal,
    long ExpenseTotal,
    long Net,
    int TransactionsCount
);
