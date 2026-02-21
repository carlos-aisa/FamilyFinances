namespace FamilyFinances.Application.Reporting.Semantics;

public enum ReportingMetricFamily
{
    Stock = 1,
    Flow = 2,
    Count = 3
}

public enum ReportingMetricKey
{
    AssetBalance = 1,
    LiabilityBalance = 2,
    NetWorth = 3,
    Income = 4,
    Expense = 5,
    PeriodNetResult = 6,
    DeltaVsPreviousMonth = 7,
    DeltaVsYearStart = 8,
    TransactionsCount = 9
}

public sealed record ReportingMetricDefinition(
    ReportingMetricKey Key,
    string CanonicalName,
    ReportingMetricFamily Family,
    string FormulaIntent);

public static class ReportingMetricSemantics
{
    private static readonly IReadOnlyDictionary<ReportingMetricKey, ReportingMetricDefinition> _definitions =
        new Dictionary<ReportingMetricKey, ReportingMetricDefinition>
        {
            [ReportingMetricKey.AssetBalance] = new(
                ReportingMetricKey.AssetBalance,
                "Asset Balance",
                ReportingMetricFamily.Stock,
                "Sum of balances for accounts where AccountNature = Asset at a point in time."),
            [ReportingMetricKey.LiabilityBalance] = new(
                ReportingMetricKey.LiabilityBalance,
                "Liability Balance",
                ReportingMetricFamily.Stock,
                "Sum of balances for accounts where AccountNature = Liability at a point in time."),
            [ReportingMetricKey.NetWorth] = new(
                ReportingMetricKey.NetWorth,
                "Net Worth",
                ReportingMetricFamily.Stock,
                "Asset Balance - Liability Balance at a point in time."),
            [ReportingMetricKey.Income] = new(
                ReportingMetricKey.Income,
                "Income",
                ReportingMetricFamily.Flow,
                "Total income over the selected period."),
            [ReportingMetricKey.Expense] = new(
                ReportingMetricKey.Expense,
                "Expense",
                ReportingMetricFamily.Flow,
                "Total expenses over the selected period."),
            [ReportingMetricKey.PeriodNetResult] = new(
                ReportingMetricKey.PeriodNetResult,
                "Period Net Result",
                ReportingMetricFamily.Flow,
                "Income + Expense over the selected period."),
            [ReportingMetricKey.DeltaVsPreviousMonth] = new(
                ReportingMetricKey.DeltaVsPreviousMonth,
                "Delta vs Previous Month",
                ReportingMetricFamily.Stock,
                "Current end balance minus previous month end balance."),
            [ReportingMetricKey.DeltaVsYearStart] = new(
                ReportingMetricKey.DeltaVsYearStart,
                "Delta vs Year Start",
                ReportingMetricFamily.Stock,
                "Current end balance minus year-start baseline balance."),
            [ReportingMetricKey.TransactionsCount] = new(
                ReportingMetricKey.TransactionsCount,
                "Transactions Count",
                ReportingMetricFamily.Count,
                "Number of transactions in the selected period.")
        };

    private static readonly IReadOnlyDictionary<string, ReportingMetricKey> _kpiToMetric =
        new Dictionary<string, ReportingMetricKey>(StringComparer.OrdinalIgnoreCase)
        {
            ["monthly-summary-income"] = ReportingMetricKey.Income,
            ["monthly-summary-expense"] = ReportingMetricKey.Expense,
            ["monthly-summary-period-net-result"] = ReportingMetricKey.PeriodNetResult,
            ["monthly-summary-transactions"] = ReportingMetricKey.TransactionsCount,
            ["monthly-evolution-latest-asset-end-balance"] = ReportingMetricKey.AssetBalance,
            ["monthly-evolution-latest-end-balance"] = ReportingMetricKey.AssetBalance,
            ["monthly-evolution-latest-asset-delta-prev-month"] = ReportingMetricKey.DeltaVsPreviousMonth,
            ["monthly-evolution-latest-delta-prev-month"] = ReportingMetricKey.DeltaVsPreviousMonth,
            ["monthly-evolution-latest-asset-delta-year-start"] = ReportingMetricKey.DeltaVsYearStart,
            ["monthly-evolution-latest-delta-year-start"] = ReportingMetricKey.DeltaVsYearStart
        };

    public static IReadOnlyDictionary<ReportingMetricKey, ReportingMetricDefinition> Definitions => _definitions;

    public static IReadOnlyDictionary<string, ReportingMetricKey> KpiToMetric => _kpiToMetric;

    public static ReportingMetricDefinition Get(ReportingMetricKey key)
    {
        if (!_definitions.TryGetValue(key, out var definition))
            throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown reporting metric key.");

        return definition;
    }

    public static bool TryResolveMetric(string kpiId, out ReportingMetricDefinition definition)
    {
        definition = default!;
        if (!_kpiToMetric.TryGetValue(kpiId, out var key))
            return false;

        definition = Get(key);
        return true;
    }

    public static ReportingMetricDefinition ResolveMetric(string kpiId)
    {
        if (TryResolveMetric(kpiId, out var definition))
            return definition;

        throw new KeyNotFoundException($"Unknown KPI mapping '{kpiId}'.");
    }
}
