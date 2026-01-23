using FamilyFinances.Application.Ledger;
using FamilyFinances.Application.Ledger.Accounts.Abstractions;
using FamilyFinances.Application.Ledger.Accounts.Dtos;
using FamilyFinances.Application.Ledger.Accounts.Requests;
using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;

namespace FamilyFinances.Application.Ledger.Accounts.Handlers;

/// <summary>
/// Reconciles an account balance by creating an adjustment transaction.
/// Never modifies historical transactions.
/// </summary>
public sealed class ReconcileAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IAccountBalanceService _balanceService;
    private readonly ILedgerUnitOfWork _uow;

    // Standard names for adjustment accounts
    private const string IncomeAdjustmentAccountName = "Balance Adjustments";
    private const string ExpenseAdjustmentAccountName = "Balance Adjustments";

    public ReconcileAccountHandler(
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IAccountBalanceService balanceService,
        ILedgerUnitOfWork uow)
    {
        _accounts = accounts;
        _transactions = transactions;
        _balanceService = balanceService;
        _uow = uow;
    }

    public async Task<ReconcileAccountResponse> HandleAsync(
        Guid accountId,
        ReconcileAccountRequest request,
        CancellationToken ct)
    {
        // 1. Validate account exists and is Asset or Liability
        var accountIdVo = new AccountId(accountId);
        var account = await _accounts.GetByIdAsync(accountIdVo, ct);

        if (account is null)
            throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        if (account.Nature != AccountNature.Asset && account.Nature != AccountNature.Liability)
            throw new DomainException($"Reconciliation is only supported for Asset and Liability accounts. Account '{account.Name}' is of type {account.Nature}.");

        if (account.IsClosed)
            throw new DomainException($"Cannot reconcile closed account '{account.Name}'.");

        // 2. Compute current balance as of the specified date (inclusive)
        var computedBalance = await ComputeBalanceAsOfAsync(accountIdVo, request.AsOfDate, ct);

        // 3. Calculate difference
        var difference = request.ActualBalance - computedBalance;

        // 4. If no difference, return early
        if (difference == 0m)
        {
            return new ReconcileAccountResponse(
                AdjustmentCreated: false,
                TransactionId: null,
                ComputedBalance: computedBalance,
                ActualBalance: request.ActualBalance,
                Difference: 0m,
                Message: "Account already reconciled. No adjustment needed."
            );
        }

        // 5. Create adjustment transaction
        var adjustmentTransaction = await CreateAdjustmentTransactionAsync(
            account,
            difference,
            request.AsOfDate,
            request.Note,
            ct);

        await _transactions.AddAsync(adjustmentTransaction, ct);
        await _uow.SaveChangesAsync(ct);

        var message = difference > 0
            ? $"Balance increased by {Math.Abs(difference):C2}"
            : $"Balance decreased by {Math.Abs(difference):C2}";

        return new ReconcileAccountResponse(
            AdjustmentCreated: true,
            TransactionId: adjustmentTransaction.Id.Value,
            ComputedBalance: computedBalance,
            ActualBalance: request.ActualBalance,
            Difference: difference,
            Message: message
        );
    }

    /// <summary>
    /// Computes the balance of an account as of a specific date (inclusive).
    /// Balance = sum of all split amounts for the account where BookedOn <= asOfDate.
    /// </summary>
    private Task<decimal> ComputeBalanceAsOfAsync(
        AccountId accountId,
        DateOnly asOfDate,
        CancellationToken ct)
    {
        return _balanceService.ComputeBalanceAsOfAsync(accountId, asOfDate, ct);
    }

    /// <summary>
    /// Creates an adjustment transaction with two splits:
    /// - One for the target account (Asset/Liability)
    /// - One for the adjustment account (Income or Expense)
    /// 
    /// The split amount for the target account equals the difference.
    /// This directly adjusts the balance: currentBalance + splitAmount = actualBalance
    /// </summary>
    private async Task<Transaction> CreateAdjustmentTransactionAsync(
        Account targetAccount,
        decimal difference,
        DateOnly asOfDate,
        string? note,
        CancellationToken ct)
    {
        // Determine the description
        var description = string.IsNullOrWhiteSpace(note)
            ? "Balance adjustment (reconciliation)"
            : $"Balance adjustment - {note.Trim()}";

        // The split amount for the target account should equal the difference
        // This directly adjusts the balance: currentBalance + splitAmount = actualBalance
        long targetSplitAmountCents = (long)(difference * 100);
        long adjustmentSplitAmountCents = -targetSplitAmountCents; // Must balance to zero

        // Determine which adjustment account to use based on the sign
        Account adjustmentAccount;
        if (targetSplitAmountCents < 0)
        {
            // Negative split for asset = money coming in ? Income adjustment
            adjustmentAccount = await GetOrCreateAdjustmentAccountAsync(
                AccountNature.Income,
                IncomeAdjustmentAccountName,
                ct);
        }
        else
        {
            // Positive split for asset = money going out ? Expense adjustment
            adjustmentAccount = await GetOrCreateAdjustmentAccountAsync(
                AccountNature.Expense,
                ExpenseAdjustmentAccountName,
                ct);
        }

        // Create splits
        var splits = new[]
        {
            TransactionSplit.Create(
                targetAccount.Id,
                new Money(targetSplitAmountCents),
                memo: "Reconciliation adjustment"
            ),
            TransactionSplit.Create(
                adjustmentAccount.Id,
                new Money(adjustmentSplitAmountCents),
                memo: "Adjustment"
            )
        };

        // Create transaction
        return Transaction.Create(
            bookedOn: asOfDate,
            description: description,
            splits: splits,
            payeeId: null
        );
    }

    /// <summary>
    /// Gets or creates an adjustment account for Income or Expense nature.
    /// </summary>
    private async Task<Account> GetOrCreateAdjustmentAccountAsync(
        AccountNature nature,
        string accountName,
        CancellationToken ct)
    {
        if (nature != AccountNature.Income && nature != AccountNature.Expense)
            throw new ArgumentException($"Adjustment accounts must be Income or Expense, got {nature}", nameof(nature));

        // Try to find existing adjustment account
        var allAccounts = await _accounts.ListAsync(ct);
        var existing = allAccounts.FirstOrDefault(a =>
            a.Nature == nature &&
            a.Name.Equals(accountName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
            return existing;

        // Create new adjustment account
        var newAccount = Account.Create(
            name: accountName,
            nature: nature,
            kind: nature == AccountNature.Income
                ? AccountKind.IncomeSource
                : AccountKind.ExpenseCategory,
            openedOn: DateOnly.FromDateTime(DateTime.Today)
        );

        await _accounts.AddAsync(newAccount, ct);
        await _uow.SaveChangesAsync(ct);

        return newAccount;
    }
}

