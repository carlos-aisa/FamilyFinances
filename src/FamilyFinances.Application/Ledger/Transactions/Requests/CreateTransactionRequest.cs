namespace FamilyFinances.Application.Ledger.Transactions.Requests;

public sealed record CreateTransactionRequest(
    DateOnly BookedOn,
    string Description,
    IReadOnlyList<TransactionSplitInput> Splits,
    Guid? PayeeId,
    Guid? RelatedTransactionId = null);