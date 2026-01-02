namespace FamilyFinances.Application.Reporting.Internal;

internal static class ReportingGuards
{
    public static void EnsureValidMonth(int month)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
    }

    public static void EnsureValidYear(int year)
    {
        if (year is < 1 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
    }

    public static void EnsureValidPeriod(DateOnly fromInclusive, DateOnly toExclusive)
    {
        if (toExclusive <= fromInclusive)
            throw new ArgumentException("toExclusive must be greater than fromInclusive.");
    }
}
