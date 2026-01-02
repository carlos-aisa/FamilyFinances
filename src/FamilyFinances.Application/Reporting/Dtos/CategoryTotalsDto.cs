using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record CategoryTotalsDto(
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    AccountNature Nature,
    IReadOnlyList<CategoryTotalItemDto> Items
);

public sealed record CategoryTotalItemDto(
    Guid AccountId,
    string AccountName,
    decimal Total,
    int TransactionsCount
);
