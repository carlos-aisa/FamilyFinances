using FamilyFinances.Application.Ledger.Transactions.Abstractions;
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

        // Build new splits (Convention A)
        var money = Money.FromEuros(amount);

        var splits = new[]
        {
        TransactionSplit.Create(
            new AccountId(fromAccountId),
            new Money(-money.Cents)), // Negate amount

        TransactionSplit.Create(
            new AccountId(toAccountId),
            money)
    };

        var newTransaction = Transaction.Create(
            bookedOn,
            description,
            splits,
            payeeId is null ? null : new PayeeId(payeeId.Value));

        // Force same identity
        await RemoveAsync(new TransactionId(id), ct);
        //_db.Entry(existing).State = EntityState.Detached;

        _db.Transactions.Add(Transaction.Create(
            bookedOn,
            description,
            splits,
            payeeId is null ? null : new PayeeId(payeeId.Value),
            existing.Id.Value));

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct)
    {
        return await _db.Transactions.AnyAsync(ct);
    }
}
