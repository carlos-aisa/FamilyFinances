using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Abstractions;

public interface IReportingReadRepository
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        Guid? accountId,
        Guid? payeeId,
        CancellationToken ct);

    Task<CategoryTotalsDto> GetCategoryTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature nature,
        Guid? payeeId,
        CancellationToken ct);

    Task<AccountTotalsDto> GetAccountTotalsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        bool includeZeroAccounts,
        CancellationToken ct);

    Task<AssetTotalBalanceDto> GetAssetTotalBalanceAsync(
        DateOnly asOf,
        CancellationToken ct);

    Task<AccountGroupTotalsDto> GetAccountGroupTotalsAsync(
        Guid groupId,
        DateOnly fromInclusive,
        DateOnly toExclusive,
        AccountNature? nature,
        CancellationToken ct);

    /// <summary>
    /// Gets movements for a specific account within a date range.
    /// </summary>
    Task<AccountMovementsDto> GetAccountMovementsAsync(
        Guid accountId,
        DateOnly fromInclusive,
        DateOnly toExclusive,
        string? searchQuery = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Gets current balances for all accounts.
    /// </summary>
    Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesAsync(CancellationToken ct = default);
}
