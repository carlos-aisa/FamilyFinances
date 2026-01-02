using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Create;

public sealed record CreateAccountCommand(
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn);