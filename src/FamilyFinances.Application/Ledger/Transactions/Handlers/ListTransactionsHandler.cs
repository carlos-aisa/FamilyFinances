using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Transactions.Handlers;

public sealed class ListTransactionsHandler
{
    private readonly ITransactionRepository _repo;

    public ListTransactionsHandler(ITransactionRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<TransactionListItemDto>> HandleAsync(int take, CancellationToken ct)
    {
        var items = await _repo.ListAsync(take, ct);

        return items.Select(t =>
        {
            var expenseSplit = t.Splits
                .FirstOrDefault(s => s.Amount.Cents > 0 && s.Account.Nature == AccountNature.Expense);

            var incomeSplit = t.Splits
                .FirstOrDefault(s => s.Amount.Cents < 0 && s.Account.Nature == AccountNature.Income);

            var assetSplits = t.Splits
                .Where(s => s.Account.Nature is AccountNature.Asset or AccountNature.Liability)
                .ToList();

            TransactionListItemType type;
            string headline;

            if (expenseSplit is not null)
            {
                type = TransactionListItemType.Expense;
                headline = expenseSplit.Account.Name;
            }
            else if (incomeSplit is not null)
            {
                type = TransactionListItemType.Income;
                headline = incomeSplit.Account.Name;
            }
            else if (assetSplits.Count == 2)
            {
                type = TransactionListItemType.Transfer;
                headline = $"{assetSplits[0].Account.Name} ? {assetSplits[1].Account.Name}";
            }
            else
            {
                type = TransactionListItemType.Other;
                headline = "Other";
            }

            return new TransactionListItemDto(
                t.Id.Value,
                t.BookedOn,
                headline,
                CreateSubHeadLine(t),
                CalculateAmount(t),
                type);
        }).ToList();
    }

    private static string CreateSubHeadLine(Transaction transaction)
    {
        var payeeName = transaction.Payee?.Name;
        if (string.IsNullOrWhiteSpace(payeeName))
            return transaction.Description;
        return $"{payeeName} - {transaction.Description}";
    }

    private static decimal CalculateAmount(Transaction transaction)
    {
        // UX rule:
        // Show the absolute value of the "main" money movement.
        // Convention: sum of negative splits (money leaving an account).
        var amount = transaction.Splits
            .Where(s => s.Amount.Cents < 0)
            .Sum(s => Math.Abs(s.Amount.Cents) / 100m);

        return amount;
    }
}
