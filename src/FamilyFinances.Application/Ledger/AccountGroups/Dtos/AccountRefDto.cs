using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.AccountGroups.Dtos;

public sealed record AccountRefDto(
    Guid AccountId,
    string Name,
    AccountNature Nature,
    AccountKind Kind
);
