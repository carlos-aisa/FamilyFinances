namespace FamilyFinances.Application.Ledger.AccountGroups.Dtos;

public sealed record AccountGroupDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsDashboardPinned = false
);
