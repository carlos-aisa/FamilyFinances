using FamilyFinances.Application.Ledger.Transactions.Requests;

namespace FamilyFinances.Web.Features.Transactions;

public static class RefundTransactionFactory
{
    public static CreateTransactionRequest Build(
        DateOnly bookedOn,
        string description,
        Guid expenseAccountId,
        Guid destinationAccountId,
        decimal amount,
        Guid? payeeId,
        string? memo,
        Guid? relatedTransactionId = null)
    {
        var cents = Money.ToCents(amount);

        return new CreateTransactionRequest(
            bookedOn,
            description,
            new[]
            {
                new TransactionSplitInput(expenseAccountId, -cents, null),
                new TransactionSplitInput(destinationAccountId, +cents, memo)
            },
            PayeeId: null,  // Transfers typically don't have payees,
            RelatedTransactionId = relatedTransactionId
        );

    }

    private static class Money
    {
        public static long ToCents(decimal amount)
            => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}