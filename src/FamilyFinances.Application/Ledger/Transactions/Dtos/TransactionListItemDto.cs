namespace FamilyFinances.Application.Ledger.Transactions.Dtos;

public enum TransactionListItemType
{
    Expense,
    Income,
    Transfer,
    Refund,
    Other
}

public sealed record TransactionListItemDto(
    Guid Id,
    DateOnly BookedOn,
    string Headline,
    string? Subheadline,
    string? PayeeName,
    decimal Amount,
    TransactionListItemType Type);
