namespace FamilyFinances.Application.Ledger.Transactions.Create;

public sealed record CreateTransactionCommand(
    DateOnly BookedOn,
    string Description,
    IReadOnlyList<TransactionSplitInput> Splits);