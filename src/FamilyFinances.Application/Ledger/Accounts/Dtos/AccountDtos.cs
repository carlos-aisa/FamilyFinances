using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Dtos;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn,
    bool IsClosed,
    DateOnly? ClosedOn,
    Guid KindId = default,
    string KindKey = "",
    string KindName = "");

public sealed record AccountKindCatalogDto(
    Guid Id,
    string Key,
    string Name,
    bool IsSystem,
    bool IsActive,
    int SortOrder,
    AccountKind LegacyKind,
    AccountNature Nature = AccountNature.Expense);
