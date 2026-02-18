namespace FamilyFinances.Application.Ledger.FiscalYears.Requests;

public sealed record GetHistoricalAccountMovementsRequest(
    Guid AccountId,
    int Year,
    string? SearchQuery,
    int Page,
    int PageSize
);
