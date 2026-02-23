using System.Globalization;

namespace FamilyFinances.Web.Features.Reports;

/// <summary>
/// Helper methods for date operations in reports.
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Gets the first day of the current month in Europe/Madrid timezone.
    /// </summary>
    public static DateOnly GetCurrentMonthStart()
    {
        var madridTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var madridNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, madridTimeZone);
        return new DateOnly(madridNow.Year, madridNow.Month, 1);
    }

    /// <summary>
    /// Gets the first day of the next month (exclusive end for current month range).
    /// </summary>
    public static DateOnly GetCurrentMonthEnd()
    {
        var start = GetCurrentMonthStart();
        return start.AddMonths(1);
    }

    /// <summary>
    /// Gets the current year in Europe/Madrid timezone.
    /// </summary>
    public static int GetCurrentYear()
    {
        var madridTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var madridNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, madridTimeZone);
        return madridNow.Year;
    }

    /// <summary>
    /// Gets the current month in Europe/Madrid timezone.
    /// </summary>
    public static int GetCurrentMonth()
    {
        var madridTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var madridNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, madridTimeZone);
        return madridNow.Month;
    }

    /// <summary>
    /// Gets the month name from a month number using active culture.
    /// </summary>
    public static string GetMonthName(int month, CultureInfo? culture = null)
    {
        return new DateTime(2000, month, 1).ToString("MMMM", culture ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Gets the first day of a given month.
    /// </summary>
    public static DateOnly GetMonthStart(int year, int month)
    {
        return new DateOnly(year, month, 1);
    }

    /// <summary>
    /// Gets the first day of the next month (exclusive end).
    /// </summary>
    public static DateOnly GetMonthEnd(int year, int month)
    {
        return new DateOnly(year, month, 1).AddMonths(1);
    }
}
