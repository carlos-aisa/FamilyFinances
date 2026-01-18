namespace FamilyFinances.Application.Ledger.Transactions.Dtos;

public sealed record ExpenseSearchResultDto(
    Guid Id,
    string Description,
    DateOnly BookedOn,
    string? PayeeName,
    decimal Amount,
    string ExpenseAccountName);