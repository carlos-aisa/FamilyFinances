namespace FamilyFinances.Application.Ledger.FiscalYears.Requests;

public sealed record ListHistoricalTransactionsRequest(
    int Year,
    int Take
);
