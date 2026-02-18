using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class ReportingReadRepository : IReportingReadRepository
{
    private readonly LedgerDbContext _db;
    private readonly IFiscalYearGovernanceRepository _governance;

    public ReportingReadRepository(
        LedgerDbContext db,
        IFiscalYearGovernanceRepository governance)
    {
        _db = db;
        _governance = governance;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct)
    {
        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking() on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking() on s.AccountId equals a.Id
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
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
        {
            // Materialize first to avoid translation issues with AccountId value object
            var allData = await q.ToListAsync(ct);
            var filtered = allData.Where(x => x.AccountId.Value == accountId.Value).ToList();
            
            // For account-level view compute inflows / outflows for the selected account:
            // - inflow: sum of positive split amounts (account increased)
            // - outflow: sum of negative split amounts (account decreased)
            var inflowCents = filtered
                .Where(x => x.Amount.Cents > 0)
                .Sum(x => x.Amount.Cents);

            var outflowCents = filtered
                .Where(x => x.Amount.Cents < 0)
                .Sum(x => x.Amount.Abs().Cents);

            var transactionsCount = filtered
                .Select(x => x.TransactionId)
                .Distinct()
                .Count();

            return new MonthlySummaryDto(
                From: fromInclusive,
                To: toExclusive,
                IncomeTotal: inflowCents,
                ExpenseTotal: outflowCents,
                Net: inflowCents - outflowCents,
                TransactionsCount: transactionsCount
            );
        }

        // Materialize the query to perform aggregations in memory
        var data = await q.ToListAsync(ct);

        // Sign convention for user-friendly display:
        // - Income accounts have NEGATIVE splits (credit) → negate to show as POSITIVE
        // - Expense accounts have POSITIVE splits (debit) → negate to show as NEGATIVE
        var incomeCentsTotal = data
            .Where(x => x.Nature == AccountNature.Income)
            .Sum(x => -x.Amount.Cents); // Negate: stored as negative, display as positive

        var expenseCentsTotal = data
            .Where(x => x.Nature == AccountNature.Expense)
            .Sum(x => -x.Amount.Cents); // Negate: stored as positive, display as negative

        var transactionsCountTotal = data
            .Select(x => x.TransactionId)
            .Distinct()
            .Count();

        return new MonthlySummaryDto(
            From: fromInclusive,
            To: toExclusive,
            IncomeTotal: incomeCentsTotal,
            ExpenseTotal: expenseCentsTotal,
            Net: incomeCentsTotal + expenseCentsTotal, // Now: positive income + negative expenses
            TransactionsCount: transactionsCountTotal
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
            .Select(g =>
            {
                // Sign convention for user-friendly display:
                // - Income accounts: stored as NEGATIVE (credit) → negate to show as POSITIVE
                // - Expense accounts: stored as POSITIVE (debit) → negate to show as NEGATIVE
                // Refunds (negative expense splits) will naturally make expenses less negative
                var signedSum = g.Sum(x => x.Amount.Cents);
                var displayTotal = -signedSum; // Always negate for consistent sign convention
                
                return new CategoryTotalItemDto(
                    g.Key.AccountId.Value,
                    g.Key.AccountName,
                    displayTotal,
                    g.Select(x => x.TransactionId).Distinct().Count()
                );
            })
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
        AccountNature? nature,
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
                nature ?? AccountNature.Expense, // Default for empty result
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
            select new
            {
                TransactionId = t.Id,
                AccountId = a.Id,
                AccountName = a.Name,
                AccountNature = a.Nature,
                Amount = s.Amount
            };

        // Filter by nature if specified (null means include all natures)
        if (nature.HasValue)
        {
            q = q.Where(x => x.AccountNature == nature.Value);
        }

        // Filter by group membership
        q = q.Where(x => accountIds.Contains(x.AccountId));

        // Materialize to avoid translation issues 
        var data = await q.ToListAsync(ct);

        var items = data
            .GroupBy(x => new { x.AccountId, x.AccountName })
            .Select(g =>
            {
                // Sign convention for user-friendly display:
                // - Income accounts: stored as NEGATIVE (credit) → negate to show as POSITIVE
                // - Expense accounts: stored as POSITIVE (debit) → negate to show as NEGATIVE
                // Refunds (negative expense splits) will naturally make expenses less negative
                var signedSum = g.Sum(x => x.Amount.Cents);
                var displayTotal = -signedSum; // Always negate for consistent sign convention
                
                return new AccountGroupTotalItemDto(
                    g.Key.AccountId.Value,
                    g.Key.AccountName,
                    displayTotal,
                    g.Select(x => x.TransactionId).Distinct().Count()
                );
            })
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
            nature ?? AccountNature.Expense, // Return the queried nature or default
            total,
            txCount,
            accountsCount,
            items
        );
    }

    /// <summary>
    /// Gets movements for a specific account within a date range.
    /// Signed amount is positive if money flows into the account, negative if money flows out.
    /// </summary>
    public async Task<AccountMovementsDto> GetAccountMovementsAsync(
        Guid accountId,
        DateOnly fromInclusive,
        DateOnly toExclusive,
        string? searchQuery = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var accountIdVo = new AccountId(accountId);

        // Find the account name (for response)
        var account = await _db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountIdVo)
            .Select(a => a.Name)
            .FirstOrDefaultAsync(ct);

        if (account is null)
            throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        // Query: transactions + splits for the requested account + payees
        var q =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join p in _db.Payees.AsNoTracking()
                on t.PayeeId equals p.Id into payees
            from payee in payees.DefaultIfEmpty()
            where s.AccountId == accountIdVo
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
            select new
            {
                TransactionId = t.Id,
                BookedOn = t.BookedOn,
                CreatedAt = t.CreatedAt,
                Description = t.Description,
                PayeeName = payee != null ? payee.Name : null,
                SignedAmount = s.Amount // This is the signed amount relative to this account
            };

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchLower = searchQuery.Trim().ToLower();
            q = q.Where(x => 
                x.Description.ToLower().Contains(searchLower) ||
                (x.PayeeName != null && x.PayeeName.ToLower().Contains(searchLower)));
        }

        // Get total count first (before pagination)
        var totalCount = await q.CountAsync(ct);

        // Apply ordering and pagination
        var movements = await q
            .OrderByDescending(x => x.BookedOn)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.TransactionId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        var movementItems = new List<AccountMovementDto>();
        var startingBalanceCents = await GetStartingBalanceCentsAsync(accountIdVo, fromInclusive, ct);

        if (movements.Count > 0)
        {
            var newestInPage = movements.First().BookedOn;

            var allMovementsForBalance = await (
                from t in _db.Transactions.AsNoTracking()
                join s in _db.TransactionSplits.AsNoTracking()
                    on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
                where s.AccountId == accountIdVo
                where t.BookedOn >= fromInclusive && t.BookedOn <= newestInPage
                orderby t.BookedOn, t.CreatedAt, t.Id
                select new { t.Id, AmountCents = s.Amount.Cents }
            ).ToListAsync(ct);

            var runningBalanceCents = startingBalanceCents;
            var balanceByTransaction = new Dictionary<TransactionId, long>();

            foreach (var m in allMovementsForBalance)
            {
                runningBalanceCents += m.AmountCents;
                balanceByTransaction[m.Id] = runningBalanceCents;
            }

            var pageTransactionIds = movements
                .Select(m => m.TransactionId)
                .Distinct()
                .ToList();

            var counterpartyRows = await (
                from s in _db.TransactionSplits.AsNoTracking()
                join a in _db.Accounts.AsNoTracking() on s.AccountId equals a.Id
                where pageTransactionIds.Contains(EF.Property<TransactionId>(s, "TransactionId"))
                where s.AccountId != accountIdVo
                select new
                {
                    TransactionId = EF.Property<TransactionId>(s, "TransactionId"),
                    AccountName = a.Name
                }
            ).ToListAsync(ct);

            var counterpartyByTransaction = counterpartyRows
                .GroupBy(x => x.TransactionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.AccountName).OrderBy(x => x).FirstOrDefault());

            foreach (var movement in movements)
            {
                counterpartyByTransaction.TryGetValue(movement.TransactionId, out var counterpartyAccountName);
                var runningBalanceForTx = balanceByTransaction.GetValueOrDefault(movement.TransactionId, startingBalanceCents);

                movementItems.Add(new AccountMovementDto(
                    movement.TransactionId.Value,
                    movement.BookedOn,
                    movement.Description,
                    movement.PayeeName,
                    movement.SignedAmount.ToEuros(),
                    counterpartyAccountName,
                    new Money(runningBalanceForTx).ToEuros()
                ));
            }
        }

        return new AccountMovementsDto(
            accountId,
            account,
            fromInclusive,
            toExclusive,
            movementItems,
            totalCount
        );
    }

    private async Task<long> GetStartingBalanceCentsAsync(
        AccountId accountId,
        DateOnly fromInclusive,
        CancellationToken ct)
    {
        var snapshot = await _governance.GetLatestSnapshotBeforeYearAsync(
            accountId.Value,
            fromInclusive.Year,
            ct);

        if (snapshot is null)
            return await SumAccountSplitsInRangeAsync(accountId, null, fromInclusive, ct);

        var deltaFrom = new DateOnly(snapshot.Value.Year + 1, 1, 1);
        if (deltaFrom >= fromInclusive)
            return snapshot.Value.ClosingBalanceCents;

        var delta = await SumAccountSplitsInRangeAsync(accountId, deltaFrom, fromInclusive, ct);
        return snapshot.Value.ClosingBalanceCents + delta;
    }

    private async Task<long> SumAccountSplitsInRangeAsync(
        AccountId accountId,
        DateOnly? fromInclusive,
        DateOnly toExclusive,
        CancellationToken ct)
    {
        var cents = await (
            from s in _db.TransactionSplits.AsNoTracking()
            join t in _db.Transactions.AsNoTracking()
                on EF.Property<TransactionId>(s, "TransactionId") equals t.Id
            where s.AccountId == accountId
            where t.BookedOn < toExclusive
            where fromInclusive == null || t.BookedOn >= fromInclusive.Value
            select s.Amount.Cents
        ).ToListAsync(ct);

        return cents.Sum();
    }

    /// <summary>
    /// Gets current balances for all accounts.
    /// Balance is computed as the sum of all transaction splits for each account.
    /// </summary>
    public async Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesAsync(CancellationToken ct = default)
    {
        // Materialize all splits first to avoid EF translation issues with Money value object
        var allSplits = await _db.TransactionSplits
            .AsNoTracking()
            .Select(s => new
            {
                AccountId = s.AccountId,
                AmountCents = s.Amount.Cents
            })
            .ToListAsync(ct);

        // Group and sum in memory
        var balances = allSplits
            .GroupBy(s => s.AccountId)
            .Select(g => new AccountBalanceDto(
                g.Key.Value,
                g.Sum(x => x.AmountCents) / 100m // Convert cents to euros
            ))
            .ToList();

        return balances;
    }
}
