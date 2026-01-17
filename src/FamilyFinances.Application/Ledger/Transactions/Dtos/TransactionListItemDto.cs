namespace FamilyFinances.Application.Ledger.Transactions.Dtos;

public enum TransactionListItemType
{
    Expense,
    Income,
    Transfer,
    Other
}

public sealed record TransactionListItemDto(
    Guid Id,
    DateOnly BookedOn,
    string Headline,
    string? Subheadline,
    decimal Amount,
    TransactionListItemType Type);
