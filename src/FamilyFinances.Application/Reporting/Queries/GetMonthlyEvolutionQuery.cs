using FamilyFinances.Application.Reporting.Dtos;

namespace FamilyFinances.Application.Reporting.Queries;

public sealed record GetMonthlyEvolutionQuery(
    int Year,
    MonthlyEvolutionScope Scope
);
