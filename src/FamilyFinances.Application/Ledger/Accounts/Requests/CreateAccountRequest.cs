using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Requests;

public sealed record CreateAccountRequest(
    string Name,
    AccountNature Nature,
    AccountKind Kind,
    DateOnly OpenedOn,
    Guid? KindId = null);