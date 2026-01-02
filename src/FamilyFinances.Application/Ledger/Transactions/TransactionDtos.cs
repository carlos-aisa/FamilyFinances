namespace FamilyFinances.Application.Ledger.Transactions;

public sealed record TransactionSplitDto(Guid AccountId, long AmountCents, string? Memo);

public sealed record TransactionDto(
    Guid Id,
    DateOnly BookedOn,
    string Description,
    Guid? PayeeId,
    IReadOnlyList<TransactionSplitDto> Splits);
