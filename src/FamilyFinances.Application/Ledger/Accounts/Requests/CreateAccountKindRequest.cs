using FamilyFinances.Domain.Ledger.Accounts;

namespace FamilyFinances.Application.Ledger.Accounts.Requests;

public sealed record CreateAccountKindRequest(string Name, AccountNature Nature);
