namespace FamilyFinances.Application.Ledger.FiscalYears.Requests;

public sealed record ReopenFiscalYearRequest(
    int Year,
    string? ActorUserId
);
