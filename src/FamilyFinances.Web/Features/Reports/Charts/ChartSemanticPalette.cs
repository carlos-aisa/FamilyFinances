namespace FamilyFinances.Web.Features.Reports.Charts;

public static class ChartSemanticPalette
{
    public const string Income = "income";
    public const string Expense = "expense";
    public const string Balance = "balance";
    public const string Neutral = "neutral";

    private static readonly string[] IndexedColors =
    [
        "#4fa4ff",
        "#2dd67d",
        "#ff5f75",
        "#f6b14a",
        "#56c7ff",
        "#6f42c1",
        "#0dcaf0",
        "#198754",
        "#ffc107",
        "#adb5bd"
    ];

    public static string ResolveSemantic(string semanticKey)
    {
        return Normalize(semanticKey) switch
        {
            Income => "#2dd67d",
            Expense => "#ff5f75",
            Balance => "#56c7ff",
            _ => "#4fa4ff"
        };
    }

    public static string ResolveIndexed(int index)
    {
        if (index < 0)
            return IndexedColors[0];

        return IndexedColors[index % IndexedColors.Length];
    }

    public static string ResolveForSeriesKey(string? seriesKey, int fallbackIndex = 0)
    {
        var normalized = Normalize(seriesKey);
        if (normalized.Contains("income", StringComparison.Ordinal))
            return ResolveSemantic(Income);
        if (normalized.Contains("expense", StringComparison.Ordinal))
            return ResolveSemantic(Expense);
        if (normalized.Contains("net", StringComparison.Ordinal) || normalized.Contains("balance", StringComparison.Ordinal))
            return ResolveSemantic(Balance);

        return ResolveIndexed(fallbackIndex);
    }

    private static string Normalize(string? key)
    {
        return string.IsNullOrWhiteSpace(key)
            ? string.Empty
            : key.Trim().ToLowerInvariant();
    }
}
