using FamilyFinances.Application.Ledger.Transactions.Abstractions;
using FamilyFinances.Application.Ledger.Transactions.Dtos;
using FamilyFinances.Application.Ledger.Transactions.Requests;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly LedgerDbContext _db;

    public TransactionRepository(LedgerDbContext db) => _db = db;

    public Task AddAsync(Transaction transaction, CancellationToken ct)
        => _db.Transactions.AddAsync(transaction, ct).AsTask();

    public Task<Transaction?> GetByIdAsync(TransactionId id, CancellationToken ct)
        => _db.Transactions
            .AsNoTracking()
            .Include(t => t.Payee)
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Transaction>> ListAsync(int take, CancellationToken ct)
        => await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Payee)
            .Include(t => t.Splits).ThenInclude(s => s.Account)
            .OrderByDescending(t => t.BookedOn)
            .ThenByDescending(t => t.Id)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Transaction>> ListByPeriodAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        int take,
        CancellationToken ct)
        => await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Payee)
            .Include(t => t.Splits).ThenInclude(s => s.Account)
            .Where(t => t.BookedOn >= fromInclusive && t.BookedOn < toExclusive)
            .OrderByDescending(t => t.BookedOn)
            .ThenByDescending(t => t.Id)
            .Take(take)
            .ToListAsync(ct);

    public async Task RemoveAsync(TransactionId id, CancellationToken ct)
    {
        var transaction = await GetByIdAsync(id, ct);
        if (transaction is not null)
        {
            _db.Transactions.Remove(transaction);
            return;
        }
    }

    public async Task<bool> UpdateTwoSplitAsync(
        Guid id,
        DateOnly bookedOn,
        string description,
        Guid? payeeId,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        CancellationToken ct)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero.");

        if (fromAccountId == toAccountId)
            throw new InvalidOperationException("From and To accounts must be different.");

        var existing = await GetByIdAsync(new TransactionId(id), ct);
        if (existing is null)
            return false;

        if (existing.Splits.Count != 2)
            throw new InvalidOperationException("Only 2-split transactions can be edited.");

        var money = Money.FromEuros(amount);

        var splits = new[]
        {
            TransactionSplit.Create(
                new AccountId(fromAccountId),
                new Money(-money.Cents)),

            TransactionSplit.Create(
                new AccountId(toAccountId),
                money)
        };

        var newTransaction = Transaction.Create(
            bookedOn,
            description,
            splits,
            payeeId is null ? null : new PayeeId(payeeId.Value));

        await RemoveAsync(new TransactionId(id), ct);

        _db.Transactions.Add(Transaction.Create(
            bookedOn,
            description,
            splits,
            payeeId is null ? null : new PayeeId(payeeId.Value),
            existing.Id.Value));

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateMultiSplitAsync(
        Guid id,
        DateOnly bookedOn,
        string description,
        Guid? payeeId,
        IReadOnlyList<TransactionSplitInput> splits,
        CancellationToken ct)
    {
        var existing = await GetByIdAsync(new TransactionId(id), ct);
        if (existing is null)
            return false;

        var domainSplits = splits.Select(s =>
            TransactionSplit.Create(new AccountId(s.AccountId), new Money(s.AmountCents), s.Memo));

        await RemoveAsync(new TransactionId(id), ct);

        _db.Transactions.Add(Transaction.Create(
            bookedOn,
            description,
            domainSplits,
            payeeId is null ? null : new PayeeId(payeeId.Value),
            existing.Id.Value));

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct)
    {
        return await _db.Transactions.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<ExpenseSearchResultDto>> SearchExpensesAsync(
        string query,
        Guid? expenseAccountId,
        int limit,
        CancellationToken ct)
    {
        var queryLower = query.ToLowerInvariant();

        var expenseTransactions = _db.Transactions
            .AsNoTracking()
            .Include(t => t.Payee)
            .Include(t => t.Splits)
                .ThenInclude(s => s.Account)
            .Where(t => t.Splits.Any(s => s.Account.Nature == AccountNature.Expense));

        // Filter by expense account if provided
        if (expenseAccountId.HasValue)
        {
            expenseTransactions = expenseTransactions
                .Where(t => t.Splits.Any(s => s.AccountId == new AccountId(expenseAccountId.Value)));
        }

        // Search by description, payee name, or expense account name
        var results = await expenseTransactions
            .Where(t =>
                t.Description.ToLower().Contains(queryLower) ||
                (t.Payee != null && t.Payee.Name.ToLower().Contains(queryLower)) ||
                t.Splits.Any(s => s.Account.Nature == AccountNature.Expense 
                    && s.Account.Name.ToLower().Contains(queryLower)))
            .OrderByDescending(t => t.BookedOn)
            .ThenByDescending(t => t.Id)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Description,
                t.BookedOn,
                PayeeName = t.Payee != null ? t.Payee.Name : null,
                ExpenseSplit = t.Splits.First(s => s.Account.Nature == AccountNature.Expense)
            })
            .ToListAsync(ct);

        return results.Select(r => new ExpenseSearchResultDto(
            r.Id.Value,
            r.Description,
            r.BookedOn,
            r.PayeeName,
            Math.Abs(r.ExpenseSplit.Amount.ToEuros()),
            r.ExpenseSplit.Account.Name
        )).ToList();
    }
}
