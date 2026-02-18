namespace FamilyFinances.Application.Ledger.FiscalYears.Requests;

public sealed record CloseFiscalYearRequest(
    int Year,
    string? ActorUserId
);
