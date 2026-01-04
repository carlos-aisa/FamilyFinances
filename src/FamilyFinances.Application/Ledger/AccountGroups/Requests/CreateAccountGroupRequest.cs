namespace FamilyFinances.Application.Ledger.AccountGroups.Requests;

public sealed record CreateAccountGroupRequest(
    string Name,
    string? Description
);