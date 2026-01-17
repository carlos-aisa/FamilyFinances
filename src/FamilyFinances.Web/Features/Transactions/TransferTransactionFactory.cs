using FamilyFinances.Application.Ledger.Transactions.Requests;

namespace FamilyFinances.Web.Features.Transactions;

public static class TransferTransactionFactory
{
    public static CreateTransactionRequest Build(
        DateOnly bookedOn,
        string description,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string? memo)
    {
        var cents = Money.ToCents(amount);

        return new CreateTransactionRequest(
            bookedOn,
            description,
            new[]
            {
                new TransactionSplitInput(fromAccountId, -cents, null),
                new TransactionSplitInput(toAccountId, +cents, memo)
            },
            PayeeId: null  // Transfers typically don't have payees
        );
    }

    private static class Money
    {
        public static long ToCents(decimal amount)
            => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}
