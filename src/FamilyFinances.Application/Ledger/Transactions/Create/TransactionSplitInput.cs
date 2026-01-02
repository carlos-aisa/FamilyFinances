namespace FamilyFinances.Application.Ledger.Transactions.Create;

public sealed record TransactionSplitInput(Guid AccountId, long AmountCents, string? Memo);