using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Domain.Common;
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
            // Expense splits: positive = normal expense, negative = refund (reduces expense)
            var expenseSplitPositive = t.Splits
                .FirstOrDefault(s => s.Amount.Cents > 0 && s.Account.Nature == AccountNature.Expense);

            var expenseSplitNegative = t.Splits
                .FirstOrDefault(s => s.Amount.Cents < 0 && s.Account.Nature == AccountNature.Expense);

            var incomeSplit = t.Splits
                .FirstOrDefault(s => s.Amount.Cents < 0 && s.Account.Nature == AccountNature.Income);

            var assetSplits = t.Splits
                .Where(s => s.Account.Nature is AccountNature.Asset or AccountNature.Liability)
                .ToList();

            // Check for Liability splits (e.g., mortgage payments)
            var liabilitySplit = t.Splits
                .FirstOrDefault(s => s.Amount.Cents > 0 && s.Account.Nature == AccountNature.Liability);

            TransactionListItemType type;
            string headline;

            // Refund detection: expense account decreased and asset account increased
            if (expenseSplitNegative is not null && assetSplits.Any(s => s.Amount.Cents > 0))
            {
                type = TransactionListItemType.Refund;
                headline = expenseSplitNegative.Account.Name;
            }
            else if (liabilitySplit is not null)
            {
                // Mortgage/liability payment: show the liability account name (e.g., "Santa Isabel")
                type = TransactionListItemType.Expense;
                headline = liabilitySplit.Account.Name;
            }
            else if (expenseSplitPositive is not null)
            {
                type = TransactionListItemType.Expense;
                headline = expenseSplitPositive.Account.Name;
            }
            else if (incomeSplit is not null)
            {
                type = TransactionListItemType.Income;
                headline = incomeSplit.Account.Name;
            }
            else if (assetSplits.Count == 2)
            {
                type = TransactionListItemType.Transfer;
                var fromSplit = assetSplits.First(s => s.Amount.Cents < 0);
                var toSplit = assetSplits.First(s => s.Amount.Cents > 0);
                headline = $"{fromSplit.Account.Name} -> {toSplit.Account.Name}";
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
        // UX rule: show absolute value of money leaving accounts (sum of negative splits)
        var totalCents = transaction.Splits
            .Where(s => s.Amount.Cents < 0)
            .Sum(s => s.Amount.Abs().Cents);

        return new Money(totalCents).ToEuros();
    }
}
