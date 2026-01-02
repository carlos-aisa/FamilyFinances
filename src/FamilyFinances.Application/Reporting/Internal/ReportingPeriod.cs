namespace FamilyFinances.Application.Reporting.Internal;

public readonly record struct ReportingPeriod(DateOnly FromInclusive, DateOnly ToExclusive)
{
    public static ReportingPeriod ForMonth(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);
        return new ReportingPeriod(from, to);
    }
}
