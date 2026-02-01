namespace FamilyFinances.Application.Ledger.Transactions.Requests;

public sealed record UpdateMultiSplitTransactionRequest(
    Guid Id,
    DateOnly BookedOn,
    string Description,
    Guid? PayeeId,
    IReadOnlyList<TransactionSplitInput> Splits);
