using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Reporting.Dtos;

public sealed record AccountTotalsDto(
    DateOnly FromInclusive,
    DateOnly ToExclusive,
    IReadOnlyList<AccountTotalItemDto> Items
);

public sealed record AccountTotalItemDto(
    Guid AccountId,
    string AccountName,
    AccountNature AccountNature,
    AccountKind AccountKind,
    long NetChange,
    int TransactionsCount
);
