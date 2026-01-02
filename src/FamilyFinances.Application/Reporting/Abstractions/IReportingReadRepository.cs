using FamilyFinances.Application.Reporting.Dtos;
using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Abstractions;

public interface IReportingReadRepository
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(
        int year,
        int month,
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
}
