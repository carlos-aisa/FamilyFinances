using FamilyFinances.Application.Ledger.Transactions.Requests;

namespace FamilyFinances.Web.Features.Transactions;

public static class ExpenseTransactionFactory
{
    public static CreateTransactionRequest Build(
        DateOnly bookedOn,
        string description,
        Guid fromAccountId,
        Guid expenseAccountId,
        decimal amount,
        Guid? payeeId,
        string? memo)
    {
        var cents = Money.ToCents(amount);

        return new CreateTransactionRequest(
            bookedOn,
            description,
            new[]
            {
                new TransactionSplitInput(fromAccountId, -cents, null),
                new TransactionSplitInput(expenseAccountId, +cents, memo)
            },
            payeeId
        );
    }

    private static class Money
    {
        public static long ToCents(decimal amount)
            => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}
