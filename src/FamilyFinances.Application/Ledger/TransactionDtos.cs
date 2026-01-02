namespace FamilyFinances.Application.Ledger;

public sealed record TransactionSplitDto(Guid AccountId, long AmountCents, string? Memo);

public sealed record TransactionDto(
    Guid Id,
    DateOnly BookedOn,
    string Description,
    IReadOnlyList<TransactionSplitDto> Splits);
