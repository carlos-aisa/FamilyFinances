namespace FamilyFinances.Web.Features.Reports.Charts;

public sealed record AnnualChartPoint(
    int Month,
    decimal Value
);

public sealed record AnnualChartSeries(
    string Key,
    string Label,
    string ColorHex,
    IReadOnlyList<AnnualChartPoint> Points
);

public sealed record AnnualCompositionSlice(
    string Key,
    string Label,
    long RawValueCents,
    decimal Percentage,
    string ColorHex
);

public static class AnnualChartPalette
{
    private static readonly string[] Colors =
    {
        "#0d6efd",
        "#20c997",
        "#dc3545",
        "#fd7e14",
        "#6f42c1",
        "#0dcaf0",
        "#198754",
        "#ffc107",
        "#6610f2",
        "#adb5bd"
    };

    public static string Resolve(int index)
    {
        if (index < 0)
            return Colors[0];

        return Colors[index % Colors.Length];
    }
}

public static class AnnualChartDefaults
{
    public const int Width = 960;
    public const int Height = 320;
    public const int PlotMarginTop = 24;
    public const int PlotMarginRight = 20;
    public const int PlotMarginBottom = 44;
    public const int PlotMarginLeft = 64;
    public const int YTicks = 5;
}
