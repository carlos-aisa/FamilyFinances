namespace FamilyFinances.Application.Ledger.Transactions.Dtos;

public sealed record TransactionSplitDto(Guid AccountId, decimal Amount, string? Memo);

public sealed record TransactionDto(
    Guid Id,
    DateOnly BookedOn,
    string Description,
    Guid? PayeeId,
    string? PayeeName,
    IReadOnlyList<TransactionSplitDto> Splits);
