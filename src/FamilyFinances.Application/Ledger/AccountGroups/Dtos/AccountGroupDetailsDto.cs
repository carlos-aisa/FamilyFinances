namespace FamilyFinances.Application.Ledger.AccountGroups.Dtos;

public sealed record AccountGroupDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<AccountRefDto> Accounts,
    bool IsDashboardPinned = false
);
