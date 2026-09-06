using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class ListLatestExpenseMovementsHandler
{
    public const int LatestExpensesCount = 6;

    private readonly ITransactionRepository _transactions;

    public ListLatestExpenseMovementsHandler(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<LatestExpenseMovementDto>> HandleAsync(CancellationToken ct)
    {
        var transactions = await _transactions.ListLatestExpensesAsync(LatestExpensesCount, ct);

        return transactions
            .Select(transaction => new LatestExpenseMovementDto(
                transaction.Id.Value,
                transaction.BookedOn,
                transaction.Description,
                transaction.Splits
                    .Where(split => split.Account.Nature == AccountNature.Expense)
                    .Sum(split => Math.Abs(split.Amount.Cents))))
            .ToList();
    }
}
