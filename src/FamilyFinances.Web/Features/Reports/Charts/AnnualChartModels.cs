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
    public static string Resolve(int index)
    {
        return ChartSemanticPalette.ResolveIndexed(index);
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
