namespace FamilyFinances.Application.Ledger.FiscalYears.Dtos;

public sealed record FiscalYearStatusDto(
    int Year,
    bool IsClosed,
    DateTime? ClosedAtUtc,
    string? ClosedByUserId,
    DateTime? ReopenedAtUtc,
    string? ReopenedByUserId
);
