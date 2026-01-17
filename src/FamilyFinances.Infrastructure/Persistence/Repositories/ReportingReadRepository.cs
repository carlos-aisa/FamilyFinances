using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class ReportingReadRepository : IReportingReadRepository
{
    private readonly LedgerDbContext _db;

    public ReportingReadRepository(LedgerDbContext db) => _db = db;

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        int year,
        int month,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct)
    {
        var fromDate = new DateOnly(year, month, 1);
        var toDate = fromDate.AddMonths(1);

        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking() on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking() on s.AccountId equals a.Id
            where t.BookedOn >= fromDate && t.BookedOn < toDate
            select new
            {
                TransactionId = t.Id,
                PayeeId = t.PayeeId,
                AccountId = s.AccountId,
                Nature = a.Nature,
                Amount = s.Amount
            };

        if (payeeId is not null)
            q = q.Where(x => x.PayeeId.HasValue && x.PayeeId.Value.Value == payeeId.Value);

        if (accountId is not null)
            q = q.Where(x => x.AccountId.Value == accountId.Value);

        // Materialize the query to perform aggregations in memory
        var data = await q.ToListAsync(ct);

        var incomeCents = data
            .Where(x => x.Nature == AccountNature.Income)
            .Sum(x => x.Amount.Abs().Cents);

        var expenseCents = data
            .Where(x => x.Nature == AccountNature.Expense)
            .Sum(x => x.Amount.Abs().Cents);

        var transactionsCount = data
            .Select(x => x.TransactionId)
            .Distinct()
            .Count();

        return new MonthlySummaryDto(
            Year: year,
            Month: month,
            IncomeTotal: incomeCents,
            ExpenseTotal: expenseCents,
            Net: incomeCents - expenseCents,
            TransactionsCount: transactionsCount
        );
    }

    public async Task<CategoryTotalsDto> GetCategoryTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        Guid? payeeId,
        CancellationToken ct)
    {
        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking() on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking() on s.AccountId equals a.Id
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
            where a.Nature == nature
            select new
            {
                TransactionId = t.Id,
                PayeeId = t.PayeeId,
                AccountId = a.Id,
                AccountName = a.Name,
                Amount = s.Amount
            };

        if (payeeId is not null)
            q = q.Where(x => x.PayeeId.HasValue && x.PayeeId.Value.Value == payeeId.Value);

        // Materialize the query to perform aggregations in memory
        var data = await q.ToListAsync(ct);

        var items = data
            .GroupBy(x => new { x.AccountId, x.AccountName })
            .Select(g => new CategoryTotalItemDto(
                g.Key.AccountId.Value,
                g.Key.AccountName,
                g.Sum(x => x.Amount.Abs().Cents),
                g.Select(x => x.TransactionId).Distinct().Count()
            ))
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.AccountName)
            .ToList();

        return new CategoryTotalsDto(
            FromInclusive: fromInclusive,
            ToExclusive: toExclusive,
            Nature: nature,
            Items: items
        );
    }

    public async Task<AccountTotalsDto> GetAccountTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        bool includeZeroAccounts,
        CancellationToken ct)
    {
        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking()
                on s.AccountId equals a.Id
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
            select new
            {
                TransactionId = t.Id,
                AccountId = a.Id,
                AccountName = a.Name,
                AccountNature = a.Nature,
                AccountKind = a.Kind,
                Amount = s.Amount
            };

        // Materialize early to avoid EF translation issues with VOs + grouping
        var data = await q.ToListAsync(ct);

        var items = data
            .GroupBy(x => new
            {
                x.AccountId,
                x.AccountName,
                x.AccountNature,
                x.AccountKind
            })
            .Select(g =>
            {
                var net = g.Sum(x => x.Amount.Cents);

                return new AccountTotalItemDto(
                    g.Key.AccountId.Value,
                    g.Key.AccountName,
                    g.Key.AccountNature,
                    g.Key.AccountKind,
                    net,
                    g.Select(x => x.TransactionId).Distinct().Count()
                );
            })
            .Where(x => includeZeroAccounts || x.NetChange != 0)
            .OrderBy(x => x.AccountNature)
            .ThenBy(x => x.AccountName)
            .ToList();

        return new AccountTotalsDto(
            fromInclusive,
            toExclusive,
            items
        );
    }

    public async Task<AccountGroupTotalsDto> GetAccountGroupTotalsAsync(
        Guid groupId,
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        CancellationToken ct)
    {
        var groupIdVo = new AccountGroupId(groupId);

        // 1) Load group
        var group = await _db.AccountGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == groupIdVo, ct);

        if (group is null)
            throw new KeyNotFoundException("Account group not found.");

        // 2) Load member account ids
        var accountIds = await _db.AccountGroupMembers
            .AsNoTracking()
            .Where(m => m.GroupId == groupIdVo)
            .Select(m => m.AccountId)
            .ToListAsync(ct);

        if (accountIds.Count == 0)
        {
            return new AccountGroupTotalsDto(
                groupId,
                group.Name,
                fromInclusive,
                toExclusive,
                nature,
                0,
                0,
                0,
                Array.Empty<AccountGroupTotalItemDto>()
            );
        }

        // 3) Query transactions + splits + accounts for those accounts
        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking()
                on s.AccountId equals a.Id
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
            where a.Nature == nature
            select new
            {
                TransactionId = t.Id,
                AccountId = a.Id,
                AccountName = a.Name,
                Amount = s.Amount
            };

        // Filter by group membership
        q = q.Where(x => accountIds.Contains(x.AccountId));

        // Materialize to avoid translation issues 
        var data = await q.ToListAsync(ct);

        var items = data
            .GroupBy(x => new { x.AccountId, x.AccountName })
            .Select(g => new AccountGroupTotalItemDto(
                g.Key.AccountId.Value,
                g.Key.AccountName,
                g.Sum(x => x.Amount.Abs().Cents),
                g.Select(x => x.TransactionId).Distinct().Count()
            ))
            .OrderByDescending(x => x.TotalCents)
            .ThenBy(x => x.AccountName)
            .ToList();

        var total = items.Sum(i => i.TotalCents);
        var txCount = data.Select(x => x.TransactionId).Distinct().Count();

        var accountsCount = items.Count;

        return new AccountGroupTotalsDto(
            groupId,
            group.Name,
            fromInclusive,
            toExclusive,
            nature,
            total,
            txCount,
            accountsCount,
            items
        );
    }
}