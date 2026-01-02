using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn,
    bool IsClosed,
    DateOnly? ClosedOn);
