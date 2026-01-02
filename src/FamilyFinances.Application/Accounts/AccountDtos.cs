using FamilyFinances.Domain.Accounts;

namespace FamilyFinances.Application.Accounts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn,
    bool IsClosed,
    DateOnly? ClosedOn);
