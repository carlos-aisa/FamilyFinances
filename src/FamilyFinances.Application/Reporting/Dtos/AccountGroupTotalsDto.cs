using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AccountGroupTotalsDto(
    Guid GroupId,
    string GroupName,
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    AccountNature Nature,
    long TotalCents,
    int TransactionsCount,
    int AccountsCount,
    IReadOnlyList<AccountGroupTotalItemDto> Items
);

public sealed record AccountGroupTotalItemDto(
    Guid AccountId,
    string AccountName,
    long TotalCents,
    int TransactionsCount
);
