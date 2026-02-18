using FamilyFinances.Application.Ledger.FiscalYears.Abstractions;
using FamilyFinances.Application.Ledger.FiscalYears.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;
using FamilyFinances.Domain.Ledger.Transactions;
using FamilyFinances.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinances.Infrastructure.Persistence.Repositories;

public sealed class FiscalYearGovernanceRepository : IFiscalYearGovernanceRepository
{
    private readonly LedgerDbContext _db;

    public FiscalYearGovernanceRepository(LedgerDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FiscalYearStatusDto>> ListStatusesAsync(CancellationToken ct)
    {
        var closureRows = await _db.FiscalYearClosures
            .AsNoTracking()
            .ToListAsync(ct);

        var txYears = await _db.Transactions
            .AsNoTracking()
            .Select(t => t.BookedOn.Year)
            .Distinct()
            .ToListAsync(ct);

        var closureByYear = closureRows.ToDictionary(x => x.Year, x => x);
        var years = txYears
            .Concat(closureByYear.Keys)
            .Append(DateTime.UtcNow.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        return years
            .Select(y =>
            {
                if (closureByYear.TryGetValue(y, out var closure))
                {
                    return new FiscalYearStatusDto(
                        y,
                        closure.IsClosed,
                        closure.ClosedAtUtc,
                        closure.ClosedByUserId,
                        closure.ReopenedAtUtc,
                        closure.ReopenedByUserId);
                }

                return new FiscalYearStatusDto(
                    y,
                    false,
                    null,
                    null,
                    null,
                    null);
            })
            .ToList();
    }

    public async Task<bool> IsYearClosedAsync(int year, CancellationToken ct)
    {
        var closure = await _db.FiscalYearClosures
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Year == year, ct);

        return closure?.IsClosed == true;
    }

    public async Task CloseYearAsync(int year, string? actorUserId, CancellationToken ct)
    {
        var closure = await _db.FiscalYearClosures
            .FirstOrDefaultAsync(x => x.Year == year, ct);

        if (closure is not null && closure.IsClosed)
            return;

        var now = DateTime.UtcNow;

        if (closure is null)
        {
            closure = new FiscalYearClosure
            {
                Year = year
            };
            _db.FiscalYearClosures.Add(closure);
        }

        closure.IsClosed = true;
        closure.ClosedAtUtc = now;
        closure.ClosedByUserId = actorUserId;
        closure.ReopenedAtUtc = null;
        closure.ReopenedByUserId = null;

        var yearEnd = new DateOnly(year, 12, 31);
        var allAccountIds = await _db.Accounts
            .AsNoTracking()
            .Select(a => a.Id)
            .ToListAsync(ct);

        var balanceRows = await (
            from s in _db.TransactionSplits.AsNoTracking()
            join t in _db.Transactions.AsNoTracking()
                on EF.Property<TransactionId>(s, "TransactionId") equals t.Id
            where t.BookedOn <= yearEnd
            select new
            {
                s.AccountId,
                s.Amount
            }
        ).ToListAsync(ct);

        var balancesByAccount = balanceRows
            .GroupBy(x => x.AccountId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount.Cents));

        var existingSnapshots = _db.AccountYearSnapshots.Where(x => x.Year == year);
        _db.AccountYearSnapshots.RemoveRange(existingSnapshots);

        foreach (var accountId in allAccountIds)
        {
            _db.AccountYearSnapshots.Add(new AccountYearSnapshot
            {
                Year = year,
                AccountId = accountId,
                ClosingBalanceCents = balancesByAccount.GetValueOrDefault(accountId, 0L),
                ComputedAtUtc = now
            });
        }
    }

    public async Task ReopenYearAsync(int year, string? actorUserId, CancellationToken ct)
    {
        var closure = await _db.FiscalYearClosures
            .FirstOrDefaultAsync(x => x.Year == year, ct);

        var now = DateTime.UtcNow;

        if (closure is null)
        {
            _db.FiscalYearClosures.Add(new FiscalYearClosure
            {
                Year = year,
                IsClosed = false,
                ReopenedAtUtc = now,
                ReopenedByUserId = actorUserId
            });
            return;
        }

        if (!closure.IsClosed)
            return;

        closure.IsClosed = false;
        closure.ReopenedAtUtc = now;
        closure.ReopenedByUserId = actorUserId;

        var snapshots = _db.AccountYearSnapshots.Where(x => x.Year == year);
        _db.AccountYearSnapshots.RemoveRange(snapshots);
    }

    public async Task<(int Year, long ClosingBalanceCents)?> GetLatestSnapshotBeforeYearAsync(
        Guid accountId,
        int yearExclusive,
        CancellationToken ct)
    {
        var accountIdVo = new AccountId(accountId);

        var snapshot = await _db.AccountYearSnapshots
            .AsNoTracking()
            .Where(x => x.AccountId == accountIdVo && x.Year < yearExclusive)
            .OrderByDescending(x => x.Year)
            .FirstOrDefaultAsync(ct);

        if (snapshot is null)
            return null;

        return (snapshot.Year, snapshot.ClosingBalanceCents);
    }
}
