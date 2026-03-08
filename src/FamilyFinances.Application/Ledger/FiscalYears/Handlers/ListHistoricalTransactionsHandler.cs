using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.FiscalYears.Handlers;

public sealed class ListHistoricalTransactionsHandler
{
    private readonly ITransactionRepository _transactions;

    public ListHistoricalTransactionsHandler(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    public async Task<IReadOnlyList<TransactionListItemDto>> HandleAsync(
        ListHistoricalTransactionsRequest request,
        CancellationToken ct)
    {
        var fromInclusive = new DateOnly(request.Year, 1, 1);
        var toExclusive = new DateOnly(request.Year + 1, 1, 1);

        var items = await _transactions.ListByPeriodAsync(fromInclusive, toExclusive, request.Take, ct);
        return items.Select(ToListItem).ToList();
    }

    private static TransactionListItemDto ToListItem(Transaction t)
    {
        var expenseSplitPositive = t.Splits
            .FirstOrDefault(s => s.Amount.Cents > 0 && s.Account.Nature == AccountNature.Expense);

        var expenseSplitNegative = t.Splits
            .FirstOrDefault(s => s.Amount.Cents < 0 && s.Account.Nature == AccountNature.Expense);

        var incomeSplit = t.Splits
            .FirstOrDefault(s => s.Amount.Cents < 0 && s.Account.Nature == AccountNature.Income);

        var assetSplits = t.Splits
            .Where(s => s.Account.Nature is AccountNature.Asset or AccountNature.Liability)
            .ToList();

        var liabilitySplit = t.Splits
            .FirstOrDefault(s => s.Amount.Cents > 0 && s.Account.Nature == AccountNature.Liability);

        TransactionListItemType type;
        string headline;

        if (expenseSplitNegative is not null && assetSplits.Any(s => s.Amount.Cents > 0))
        {
            type = TransactionListItemType.Refund;
            headline = expenseSplitNegative.Account.Name;
        }
        else if (liabilitySplit is not null)
        {
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
            CreateSubHeadline(t),
            t.Payee?.Name,
            CalculateAmount(t),
            type);
    }

    private static string CreateSubHeadline(Transaction transaction)
    {
        return transaction.Description;
    }

    private static decimal CalculateAmount(Transaction transaction)
    {
        var totalCents = transaction.Splits
            .Where(s => s.Amount.Cents < 0)
            .Sum(s => s.Amount.Abs().Cents);

        return new Money(totalCents).ToEuros();
    }
}
