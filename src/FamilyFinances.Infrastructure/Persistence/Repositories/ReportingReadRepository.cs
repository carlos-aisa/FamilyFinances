using FamilyFinances.Application.Reporting.Abstractions;
using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Application.Reporting.Internal;
using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Domain.Common;
using FamilyFinances.Domain.Ledger.AccountGroups;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Payees;
using FamilyFinances.Domain.Ledger.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class ReportingReadRepository : IReportingReadRepository
{
    private const string UngroupedAccountsLabel = "Ungrouped accounts";
    private const string UnknownPayeeLabel = "Unknown/Unassigned payee";

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
        var baseQuery =
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
        {
            var payeeIdVo = new PayeeId(payeeId.Value);
            baseQuery = baseQuery.Where(x => x.PayeeId == payeeIdVo);
        }

        var q = baseQuery.Select(x => new
        {
            x.TransactionId,
            x.AccountId,
            x.Nature,
            x.Amount
        });

        if (accountId is not null)
        {
            var selectedAccountNature = await _db.Accounts
                .AsNoTracking()
                .Where(a => a.Id == new AccountId(accountId.Value))
                .Select(a => (AccountNature?)a.Nature)
                .FirstOrDefaultAsync(ct);

            // Materialize first to avoid translation issues with AccountId value object.&
            var allData = await q.ToListAsync(ct);
            var filtered = allData.Where(x => x.AccountId.Value == accountId.Value).ToList();

            long incomeCents;
            long expenseCents;

            switch (selectedAccountNature)
            {
                case AccountNature.Income:
                {
                    var incomeSigned = filtered.Sum(x => x.Amount.Cents);
                    incomeCents = -incomeSigned;
                    expenseCents = 0L;
                    break;
                }
                case AccountNature.Expense:
                {
                    var expenseSigned = filtered.Sum(x => x.Amount.Cents);
                    incomeCents = 0L;
                    expenseCents = -expenseSigned;
                    break;
                }
                default:
                    // Fallback for non-flow natures: show inflow vs outflow for selected account.
                    incomeCents = filtered
                        .Where(x => x.Amount.Cents > 0)
                        .Sum(x => x.Amount.Cents);

                    expenseCents = filtered
                        .Where(x => x.Amount.Cents < 0)
                        .Sum(x => x.Amount.Cents);
                    break;
            }

            var transactionsCount = filtered
                .Select(x => x.TransactionId)
                .Distinct()
                .Count();

            return new MonthlySummaryDto(
                From: fromInclusive,
                To: toExclusive,
                IncomeTotal: incomeCents,
                ExpenseTotal: expenseCents,
                Net: incomeCents + expenseCents,
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
        var baseQuery =
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
        {
            var payeeIdVo = new PayeeId(payeeId.Value);
            baseQuery = baseQuery.Where(x => x.PayeeId == payeeIdVo);
        }

        var q = baseQuery.Select(x => new
        {
            x.TransactionId,
            x.AccountId,
            x.AccountName,
            x.Amount
        });

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

    public async Task<AssetTotalBalanceDto> GetAssetTotalBalanceAsync(
        DateOnly asOf,
        CancellationToken ct)
    {
        var (totalCents, assetAccountsCount) = await GetBalanceByNatureAsOfAsync(
            AccountNature.Asset,
            asOf,
            ct);

        return new AssetTotalBalanceDto(
            AsOf: asOf,
            TotalCents: totalCents,
            AssetAccountsCount: assetAccountsCount
        );
    }

    public async Task<EconomicStateDto> GetEconomicStateAsync(
        DateOnly asOf,
        DateOnly periodFromInclusive,
        DateOnly periodToExclusive,
        CancellationToken ct)
    {
        var (assetsTotalCents, _) = await GetBalanceByNatureAsOfAsync(
            AccountNature.Asset,
            asOf,
            ct);

        var (liabilitiesSignedBalanceCents, _) = await GetBalanceByNatureAsOfAsync(
            AccountNature.Liability,
            asOf,
            ct);

        // Liability balances are stored with credit-normal sign; expose a user-facing owed amount.
        var liabilitiesTotalCents = -liabilitiesSignedBalanceCents;
        var netWorthCents = assetsTotalCents - liabilitiesTotalCents;

        var periodSummary = await GetMonthlySummaryAsync(
            periodFromInclusive,
            periodToExclusive,
            accountId: null,
            payeeId: null,
            ct);

        return new EconomicStateDto(
            AsOf: asOf,
            AssetsTotalCents: assetsTotalCents,
            LiabilitiesTotalCents: liabilitiesTotalCents,
            NetWorthCents: netWorthCents,
            IncomeTotalCents: periodSummary.IncomeTotal,
            ExpenseTotalCents: periodSummary.ExpenseTotal,
            PeriodNetResultCents: periodSummary.Net
        );
    }

    public async Task<MonthlyEvolutionReportDto> GetMonthlyEvolutionAsync(
        int year,
        MonthlyEvolutionScope scope,
        CancellationToken ct)
    {
        const int monthLimit = 12;
        var yearStart = new DateOnly(year, 1, 1);
        var yearEndExclusive = new DateOnly(year + 1, 1, 1);

        var accounts = await _db.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Select(a => new AccountEvolutionSeed(a.Id, a.Name, a.Nature))
            .ToListAsync(ct);

        var accountIds = accounts
            .Select(a => a.Id)
            .ToList();

        var snapshotBalancesByAccount = await _db.AccountYearSnapshots
            .AsNoTracking()
            .Where(x => x.Year == year - 1)
            .Select(x => new
            {
                x.AccountId,
                x.ClosingBalanceCents
            })
            .ToListAsync(ct);

        var snapshotByAccount = snapshotBalancesByAccount
            .ToDictionary(x => x.AccountId, x => x.ClosingBalanceCents);

        var missingSnapshotAccountIds = accountIds
            .Where(id => !snapshotByAccount.ContainsKey(id))
            .ToList();

        Dictionary<AccountId, long> fallbackBaselineByAccount;
        if (missingSnapshotAccountIds.Count == 0)
        {
            fallbackBaselineByAccount = new Dictionary<AccountId, long>();
        }
        else
        {
            var fallbackRows = await (
                from t in _db.Transactions.AsNoTracking()
                join s in _db.TransactionSplits.AsNoTracking()
                    on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
                where missingSnapshotAccountIds.Contains(s.AccountId)
                where t.BookedOn < yearStart
                select new
                {
                    s.AccountId,
                    s.Amount
                }
            ).ToListAsync(ct);

            fallbackBaselineByAccount = fallbackRows
                .GroupBy(x => x.AccountId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount.Cents));
        }

        var baselineByAccount = new Dictionary<AccountId, long>(accountIds.Count);
        foreach (var accountId in accountIds)
        {
            if (snapshotByAccount.TryGetValue(accountId, out var snapshotBalance))
            {
                baselineByAccount[accountId] = snapshotBalance;
                continue;
            }

            baselineByAccount[accountId] = fallbackBaselineByAccount.GetValueOrDefault(accountId, 0L);
        }

        var movementRows = await (
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            where t.BookedOn >= yearStart && t.BookedOn < yearEndExclusive
            select new
            {
                s.AccountId,
                Month = t.BookedOn.Month,
                s.Amount
            }
        ).ToListAsync(ct);

        var movementByAccountAndMonth = movementRows
            .GroupBy(x => (x.AccountId, x.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount.Cents));

        var accountPointsById = new Dictionary<AccountId, IReadOnlyList<MonthlyEvolutionPointDto>>(accounts.Count);
        var accountSeries = new List<MonthlyEvolutionSeriesDto>(accounts.Count);

        foreach (var account in accounts)
        {
            var baseline = baselineByAccount.GetValueOrDefault(account.Id, 0L);
            var previousEnd = baseline;
            var points = new List<MonthlyEvolutionPointDto>(monthLimit);

            for (var month = 1; month <= monthLimit; month++)
            {
                var movement = movementByAccountAndMonth.GetValueOrDefault((account.Id, month), 0L);
                var endBalance = previousEnd + movement;

                points.Add(new MonthlyEvolutionPointDto(
                    Month: month,
                    MonthEndDate: new DateOnly(year, month, DateTime.DaysInMonth(year, month)),
                    EndBalanceCents: endBalance,
                    DeltaVsPreviousMonthCents: endBalance - previousEnd,
                    DeltaVsYearStartCents: endBalance - baseline
                ));

                previousEnd = endBalance;
            }

            accountPointsById[account.Id] = points;
            accountSeries.Add(new MonthlyEvolutionSeriesDto(
                SeriesKey: $"account:{account.Id.Value:D}",
                DisplayName: account.Name,
                EntityId: account.Id.Value,
                EntityType: "account",
                Points: points
            ));
        }

        IReadOnlyList<MonthlyEvolutionSeriesDto> series = scope switch
        {
            MonthlyEvolutionScope.Accounts => accountSeries,
            MonthlyEvolutionScope.AssetTotal => BuildAssetTotalSeries(
                accounts,
                baselineByAccount,
                accountPointsById,
                year,
                monthLimit),
            MonthlyEvolutionScope.IncomeTotal => BuildIncomeTotalSeries(
                accounts,
                baselineByAccount,
                accountPointsById,
                year,
                monthLimit),
            MonthlyEvolutionScope.AccountGroups => await BuildAccountGroupSeriesAsync(
                baselineByAccount,
                accountPointsById,
                year,
                monthLimit,
                ct),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported monthly evolution scope.")
        };

        return new MonthlyEvolutionReportDto(
            Year: year,
            Scope: scope,
            Series: series
        );
    }

    public async Task<MonthlyBalanceChartDto> GetMonthlyBalanceChartAsync(
        int year,
        int month,
        Guid? accountId,
        Guid? payeeId,
        AccountNature? nature,
        CancellationToken ct)
    {
        var seedData = await BuildMonthlyChartSeedDataAsync(year, month, payeeId, ct);

        long openingBalance;
        IReadOnlyDictionary<int, long> movementByDay;
        var shouldMirrorIncomeSign = false;

        if (accountId is not null)
        {
            var selectedAccountId = new AccountId(accountId.Value);
            openingBalance = seedData.OpeningBalanceByAccount.GetValueOrDefault(selectedAccountId, 0L);
            movementByDay = BuildMovementByDayForAccounts(seedData.MovementRows, [selectedAccountId]);
        }
        else if (nature is not null)
        {
            var natureAccountIds = seedData.Accounts
                .Where(a => a.Nature == nature.Value)
                .Select(a => a.Id)
                .ToList();

            // For nature charts we expose month-focused flow totals, so the baseline starts at zero.
            openingBalance = 0L;
            movementByDay = BuildMovementByDayForAccounts(seedData.MovementRows, natureAccountIds);
            shouldMirrorIncomeSign = nature.Value == AccountNature.Income;
        }
        else
        {
            var assetAccountIds = seedData.Accounts
                .Where(a => a.Nature == AccountNature.Asset)
                .Select(a => a.Id)
                .ToList();

            openingBalance = assetAccountIds
                .Sum(id => seedData.OpeningBalanceByAccount.GetValueOrDefault(id, 0L));

            movementByDay = BuildMovementByDayForAccounts(seedData.MovementRows, assetAccountIds);
        }

        var signedPoints = MonthlyChartBucketBuilder.BuildDailyEndBalancePoints(
            year,
            month,
            openingBalance,
            movementByDay);

        var points = shouldMirrorIncomeSign
            ? signedPoints.Select(point => point with { EndBalanceCents = -point.EndBalanceCents }).ToList()
            : signedPoints;

        return new MonthlyBalanceChartDto(year, month, points);
    }

    public async Task<MonthlyBalanceVsGroupsChartDto> GetMonthlyBalanceVsGroupsChartAsync(
        int year,
        int month,
        CancellationToken ct)
    {
        var seedData = await BuildMonthlyChartSeedDataAsync(year, month, payeeId: null, ct);

        var assetAccountIds = seedData.Accounts
            .Where(a => a.Nature == AccountNature.Asset)
            .Select(a => a.Id)
            .ToList();

        var assetOpening = assetAccountIds
            .Sum(id => seedData.OpeningBalanceByAccount.GetValueOrDefault(id, 0L));

        var assetMovementByDay = BuildMovementByDayForAccounts(seedData.MovementRows, assetAccountIds);
        var assetSeries = new MonthlyChartSeriesDto(
            SeriesKey: "asset-total",
            DisplayName: "Asset Total",
            EntityId: null,
            EntityType: "scope",
            Points: MonthlyChartBucketBuilder.BuildDailyEndBalancePoints(
                year,
                month,
                assetOpening,
                assetMovementByDay));

        var groups = await _db.AccountGroups
            .AsNoTracking()
            .Select(g => new
            {
                g.Id,
                g.Name
            })
            .OrderBy(g => g.Name)
            .ThenBy(g => g.Id)
            .ToListAsync(ct);

        var memberships = await (
            from m in _db.AccountGroupMembers.AsNoTracking()
            join a in _db.Accounts.AsNoTracking() on m.AccountId equals a.Id
            where a.Nature != AccountNature.Liability
            select new
            {
                m.GroupId,
                m.AccountId
            }
        ).ToListAsync(ct);

        var accountIdsByGroup = memberships
            .GroupBy(x => x.GroupId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.AccountId)
                    .Distinct()
                    .ToList());

        var series = new List<MonthlyChartSeriesDto>(groups.Count + 1)
        {
            assetSeries
        };

        foreach (var group in groups)
        {
            if (!accountIdsByGroup.TryGetValue(group.Id, out var memberAccountIds))
                memberAccountIds = new List<AccountId>();

            var openingBalance = memberAccountIds
                .Sum(id => seedData.OpeningBalanceByAccount.GetValueOrDefault(id, 0L));

            var movementByDay = BuildMovementByDayForAccounts(seedData.MovementRows, memberAccountIds);

            series.Add(new MonthlyChartSeriesDto(
                SeriesKey: $"account-group:{group.Id.Value:D}",
                DisplayName: group.Name,
                EntityId: group.Id.Value,
                EntityType: "account-group",
                Points: MonthlyChartBucketBuilder.BuildDailyEndBalancePoints(
                    year,
                    month,
                    openingBalance,
                    movementByDay)
            ));
        }

        var alignedSeries = MonthlyChartBucketBuilder.AlignSeriesDayBuckets(year, month, series);
        return new MonthlyBalanceVsGroupsChartDto(year, month, alignedSeries);
    }

    public async Task<IReadOnlyList<InsightContributorAggregateDto>> GetInsightContributorTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        ReportingInsightDimension dimension,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct)
    {
        var rows = await LoadInsightSplitRowsAsync(
            fromInclusive,
            toExclusive,
            nature,
            accountId,
            payeeId,
            ct);

        var contributorResolver = await BuildInsightContributorResolverAsync(dimension, ct);
        var totalsByContributor = new Dictionary<InsightContributorKey, long>();

        foreach (var row in rows)
        {
            var contributor = contributorResolver(row);
            var current = totalsByContributor.GetValueOrDefault(contributor, 0L);
            totalsByContributor[contributor] = current + row.SignedDisplayAmountCents;
        }

        return totalsByContributor
            .Select(x => new InsightContributorAggregateDto(
                x.Key.EntityId,
                x.Key.DisplayName,
                x.Value))
            .OrderByDescending(x => Math.Abs(x.AmountCents))
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<InsightMonthlyContributorAggregateDto>> GetMonthlyInsightContributorTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        ReportingInsightDimension dimension,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct)
    {
        var rows = await LoadInsightSplitRowsAsync(
            fromInclusive,
            toExclusive,
            nature,
            accountId,
            payeeId,
            ct);

        var contributorResolver = await BuildInsightContributorResolverAsync(dimension, ct);
        var totalsByContributorMonth = new Dictionary<(InsightContributorKey Contributor, int Year, int Month), long>();

        foreach (var row in rows)
        {
            var contributor = contributorResolver(row);
            var key = (contributor, row.BookedOn.Year, row.BookedOn.Month);
            var current = totalsByContributorMonth.GetValueOrDefault(key, 0L);
            totalsByContributorMonth[key] = current + row.SignedDisplayAmountCents;
        }

        return totalsByContributorMonth
            .Select(x => new InsightMonthlyContributorAggregateDto(
                x.Key.Contributor.EntityId,
                x.Key.Contributor.DisplayName,
                x.Key.Year,
                x.Key.Month,
                x.Value))
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<MonthlyEvolutionSeriesDto> BuildAssetTotalSeries(
        IReadOnlyList<AccountEvolutionSeed> accounts,
        IReadOnlyDictionary<AccountId, long> baselineByAccount,
        IReadOnlyDictionary<AccountId, IReadOnlyList<MonthlyEvolutionPointDto>> accountPointsById,
        int year,
        int monthLimit)
    {
        var assetAccountIds = accounts
            .Where(a => a.Nature == AccountNature.Asset)
            .Select(a => a.Id)
            .ToList();

        var baseline = assetAccountIds.Sum(id => baselineByAccount.GetValueOrDefault(id, 0L));

        var points = BuildAggregatedPoints(
            year,
            monthLimit,
            baseline,
            assetAccountIds
                .Where(accountPointsById.ContainsKey)
                .Select(id => accountPointsById[id])
                .ToList());

        return new[]
        {
            new MonthlyEvolutionSeriesDto(
                SeriesKey: "asset-total",
                DisplayName: "Asset Total",
                EntityId: null,
                EntityType: "scope",
                Points: points)
        };
    }

    private async Task<IReadOnlyList<InsightSplitRow>> LoadInsightSplitRowsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct)
    {
        var accountIdVo = accountId is not null ? new AccountId(accountId.Value) : (AccountId?)null;
        var payeeIdVo = payeeId is not null ? new PayeeId(payeeId.Value) : (PayeeId?)null;

        var query =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking()
                on s.AccountId equals a.Id
            where t.BookedOn >= fromInclusive && t.BookedOn < toExclusive
            where a.Nature == nature
            select new
            {
                t.BookedOn,
                s.AccountId,
                t.PayeeId,
                SignedDisplayAmountCents = -s.Amount.Cents
            };

        if (accountIdVo is not null)
            query = query.Where(x => x.AccountId == accountIdVo);

        if (payeeIdVo is not null)
            query = query.Where(x => x.PayeeId == payeeIdVo);

        var rows = await query.ToListAsync(ct);

        return rows
            .Select(x => new InsightSplitRow(
                x.BookedOn,
                x.AccountId,
                x.PayeeId,
                x.SignedDisplayAmountCents))
            .ToList();
    }

    private async Task<Func<InsightSplitRow, InsightContributorKey>> BuildInsightContributorResolverAsync(
        ReportingInsightDimension dimension,
        CancellationToken ct)
    {
        return dimension switch
        {
            ReportingInsightDimension.Group => await BuildGroupContributorResolverAsync(ct),
            ReportingInsightDimension.Payee => await BuildPayeeContributorResolverAsync(ct),
            _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Unsupported insight dimension.")
        };
    }

    private async Task<Func<InsightSplitRow, InsightContributorKey>> BuildGroupContributorResolverAsync(CancellationToken ct)
    {
        var accountGroupRows = await (
            from membership in _db.AccountGroupMembers.AsNoTracking()
            join accountGroup in _db.AccountGroups.AsNoTracking()
                on membership.GroupId equals accountGroup.Id
            select new
            {
                membership.AccountId,
                GroupId = (Guid?)accountGroup.Id.Value,
                GroupName = accountGroup.Name
            }
        ).ToListAsync(ct);

        var groupByAccount = accountGroupRows
            .GroupBy(x => x.AccountId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.GroupId)
                    .Select(x => new InsightContributorKey(x.GroupId, x.GroupName))
                    .First());

        return row => groupByAccount.TryGetValue(row.AccountId, out var contributor)
            ? contributor
            : new InsightContributorKey(null, UngroupedAccountsLabel);
    }

    private async Task<Func<InsightSplitRow, InsightContributorKey>> BuildPayeeContributorResolverAsync(CancellationToken ct)
    {
        var payeeRows = await _db.Payees
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .ToListAsync(ct);

        var payeeById = payeeRows.ToDictionary(x => x.Id, x => x.Name);

        return row =>
        {
            if (row.PayeeId is { } payeeId && payeeById.TryGetValue(payeeId, out var payeeName))
                return new InsightContributorKey(payeeId.Value, payeeName);

            return new InsightContributorKey(null, UnknownPayeeLabel);
        };
    }

    private static IReadOnlyList<MonthlyEvolutionSeriesDto> BuildIncomeTotalSeries(
        IReadOnlyList<AccountEvolutionSeed> accounts,
        IReadOnlyDictionary<AccountId, long> baselineByAccount,
        IReadOnlyDictionary<AccountId, IReadOnlyList<MonthlyEvolutionPointDto>> accountPointsById,
        int year,
        int monthLimit)
    {
        var incomeAccountIds = accounts
            .Where(a => a.Nature == AccountNature.Income)
            .Select(a => a.Id)
            .ToList();

        var baseline = incomeAccountIds.Sum(id => baselineByAccount.GetValueOrDefault(id, 0L));

        var signedPoints = BuildAggregatedPoints(
            year,
            monthLimit,
            baseline,
            incomeAccountIds
                .Where(accountPointsById.ContainsKey)
                .Select(id => accountPointsById[id])
                .ToList());

        var mirroredPoints = signedPoints
            .Select(point => point with
            {
                EndBalanceCents = -point.EndBalanceCents,
                DeltaVsPreviousMonthCents = -point.DeltaVsPreviousMonthCents,
                DeltaVsYearStartCents = -point.DeltaVsYearStartCents
            })
            .ToList();

        return new[]
        {
            new MonthlyEvolutionSeriesDto(
                SeriesKey: "income-total",
                DisplayName: "Income Total",
                EntityId: null,
                EntityType: "scope",
                Points: mirroredPoints)
        };
    }

    private async Task<IReadOnlyList<MonthlyEvolutionSeriesDto>> BuildAccountGroupSeriesAsync(
        IReadOnlyDictionary<AccountId, long> baselineByAccount,
        IReadOnlyDictionary<AccountId, IReadOnlyList<MonthlyEvolutionPointDto>> accountPointsById,
        int year,
        int monthLimit,
        CancellationToken ct)
    {
        var groups = await _db.AccountGroups
            .AsNoTracking()
            .Select(g => new
            {
                g.Id,
                g.Name
            })
            .OrderBy(g => g.Name)
            .ThenBy(g => g.Id)
            .ToListAsync(ct);

        var memberships = await (
            from m in _db.AccountGroupMembers.AsNoTracking()
            join a in _db.Accounts.AsNoTracking() on m.AccountId equals a.Id
            where a.Nature != AccountNature.Liability
            select new
            {
                m.GroupId,
                m.AccountId
            }
        ).ToListAsync(ct);

        var accountIdsByGroup = memberships
            .GroupBy(x => x.GroupId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.AccountId)
                    .Distinct()
                    .ToList());

        var series = new List<MonthlyEvolutionSeriesDto>(groups.Count);
        foreach (var group in groups)
        {
            if (!accountIdsByGroup.TryGetValue(group.Id, out var memberAccountIds))
                memberAccountIds = new List<AccountId>();

            var baseline = memberAccountIds.Sum(id => baselineByAccount.GetValueOrDefault(id, 0L));

            var points = BuildAggregatedPoints(
                year,
                monthLimit,
                baseline,
                memberAccountIds
                    .Where(accountPointsById.ContainsKey)
                    .Select(id => accountPointsById[id])
                    .ToList());

            series.Add(new MonthlyEvolutionSeriesDto(
                SeriesKey: $"account-group:{group.Id.Value:D}",
                DisplayName: group.Name,
                EntityId: group.Id.Value,
                EntityType: "account-group",
                Points: points
            ));
        }

        return series;
    }

    private static IReadOnlyList<MonthlyEvolutionPointDto> BuildAggregatedPoints(
        int year,
        int monthLimit,
        long yearStartBaseline,
        IReadOnlyList<IReadOnlyList<MonthlyEvolutionPointDto>> sourceSeries)
    {
        var points = new List<MonthlyEvolutionPointDto>(monthLimit);
        var previousEnd = yearStartBaseline;

        for (var month = 1; month <= monthLimit; month++)
        {
            long endBalance = 0;
            foreach (var source in sourceSeries)
            {
                if (source.Count >= month)
                    endBalance += source[month - 1].EndBalanceCents;
            }

            points.Add(new MonthlyEvolutionPointDto(
                Month: month,
                MonthEndDate: new DateOnly(year, month, DateTime.DaysInMonth(year, month)),
                EndBalanceCents: endBalance,
                DeltaVsPreviousMonthCents: endBalance - previousEnd,
                DeltaVsYearStartCents: endBalance - yearStartBaseline
            ));

            previousEnd = endBalance;
        }

        return points;
    }

    private readonly record struct InsightContributorKey(Guid? EntityId, string DisplayName);

    private sealed record InsightSplitRow(
        DateOnly BookedOn,
        AccountId AccountId,
        PayeeId? PayeeId,
        long SignedDisplayAmountCents);

    private sealed record AccountEvolutionSeed(AccountId Id, string Name, AccountNature Nature);

    private sealed record MonthlyChartMovementRow(AccountId AccountId, int Day, long AmountCents);

    private sealed record MonthlyChartSeedData(
        IReadOnlyList<AccountEvolutionSeed> Accounts,
        IReadOnlyDictionary<AccountId, long> OpeningBalanceByAccount,
        IReadOnlyList<MonthlyChartMovementRow> MovementRows);

    private async Task<MonthlyChartSeedData> BuildMonthlyChartSeedDataAsync(
        int year,
        int month,
        Guid? payeeId,
        CancellationToken ct)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEndExclusive = monthStart.AddMonths(1);
        var payeeIdVo = payeeId is not null ? new PayeeId(payeeId.Value) : (PayeeId?)null;

        var accounts = await _db.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Select(a => new AccountEvolutionSeed(a.Id, a.Name, a.Nature))
            .ToListAsync(ct);

        var openingRows = await (
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            where t.BookedOn < monthStart
            where payeeIdVo == null || t.PayeeId == payeeIdVo
            select new
            {
                s.AccountId,
                AmountCents = s.Amount.Cents
            }
        ).ToListAsync(ct);

        var openingBalanceByAccount = openingRows
            .GroupBy(x => x.AccountId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.AmountCents));

        var movementRows = await (
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            where t.BookedOn >= monthStart && t.BookedOn < monthEndExclusive
            where payeeIdVo == null || t.PayeeId == payeeIdVo
            select new MonthlyChartMovementRow(
                s.AccountId,
                t.BookedOn.Day,
                s.Amount.Cents)
        ).ToListAsync(ct);

        return new MonthlyChartSeedData(accounts, openingBalanceByAccount, movementRows);
    }

    private static IReadOnlyDictionary<int, long> BuildMovementByDayForAccounts(
        IReadOnlyList<MonthlyChartMovementRow> movementRows,
        IReadOnlyCollection<AccountId> accountIds)
    {
        if (accountIds.Count == 0)
            return new Dictionary<int, long>();

        var accountSet = accountIds.ToHashSet();

        return movementRows
            .Where(x => accountSet.Contains(x.AccountId))
            .GroupBy(x => x.Day)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.AmountCents));
    }

    private async Task<(long TotalCents, int AccountCount)> GetBalanceByNatureAsOfAsync(
        AccountNature nature,
        DateOnly asOf,
        CancellationToken ct)
    {
        var splitsQuery =
            from t in _db.Transactions.AsNoTracking()
            join s in _db.TransactionSplits.AsNoTracking()
                on t.Id equals EF.Property<TransactionId>(s, "TransactionId")
            join a in _db.Accounts.AsNoTracking()
                on s.AccountId equals a.Id
            where a.Nature == nature
            where t.BookedOn <= asOf
            select new
            {
                AccountId = EF.Property<Guid>(s, nameof(TransactionSplit.AccountId)),
                AmountCents = EF.Property<long>(s, nameof(TransactionSplit.Amount))
            };

        var totalCents = await splitsQuery
            .Select(x => (long?)x.AmountCents)
            .SumAsync(ct) ?? 0L;

        var accountCount = await splitsQuery
            .Select(x => x.AccountId)
            .Distinct()
            .CountAsync(ct);

        return (totalCents, accountCount);
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
