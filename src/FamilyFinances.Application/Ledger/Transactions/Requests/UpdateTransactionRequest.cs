namespace FamilyFinances.Application.Ledger.Transactions.Requests
{
    public sealed record UpdateTransactionRequest(
        Guid Id,
        DateOnly BookedOn,
        string Description,
        Guid? PayeeId,
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount);
}
