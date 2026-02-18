using FamilyFinances.Application.Ledger.FiscalYears.Dtos;

namespace FamilyFinances.Application.Ledger.FiscalYears.Abstractions;

public interface IFiscalYearGovernanceRepository
{
    Task<IReadOnlyList<FiscalYearStatusDto>> ListStatusesAsync(CancellationToken ct);
    Task<bool> IsYearClosedAsync(int year, CancellationToken ct);
    Task CloseYearAsync(int year, string? actorUserId, CancellationToken ct);
    Task ReopenYearAsync(int year, string? actorUserId, CancellationToken ct);
    Task<(int Year, long ClosingBalanceCents)?> GetLatestSnapshotBeforeYearAsync(
        Guid accountId,
        int yearExclusive,
        CancellationToken ct);
}
