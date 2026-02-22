namespace FamilyFinances.Web.Features.Reports.Charts;

public sealed record MonthlyChartPoint(
    int Day,
    decimal Value
);

public sealed record MonthlyChartSeries(
    string Key,
    string Label,
    string ColorHex,
    IReadOnlyList<MonthlyChartPoint> Points
);
