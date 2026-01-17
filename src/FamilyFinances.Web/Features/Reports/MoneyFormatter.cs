using System.Globalization;

namespace FamilyFinances.Web.Features.Reports;

/// <summary>
/// Formats monetary values from cents to currency display.
/// </summary>
public static class MoneyFormatter
{
    /// <summary>
    /// Formats cents to a currency string (e.g., 123456 -> "€1,234.56").
    /// </summary>
    /// <param name="cents">The amount in cents.</param>
    /// <param name="showCurrency">Whether to include the currency symbol.</param>
    /// <returns>Formatted currency string.</returns>
    public static string FormatCents(long cents, bool showCurrency = true)
    {
        var amount = cents / 100m;
        var formatted = amount.ToString("N2", CultureInfo.InvariantCulture); // Two decimals with thousand separators

        return showCurrency ? $"€{formatted}" : formatted;
    }

    /// <summary>
    /// Formats cents to a currency string with sign indication for positive values.
    /// Useful for income/expense displays.
    /// </summary>
    /// <param name="cents">The amount in cents.</param>
    /// <param name="showCurrency">Whether to include the currency symbol.</param>
    /// <returns>Formatted currency string with sign.</returns>
    public static string FormatCentsWithSign(long cents, bool showCurrency = true)
    {
        var sign = cents > 0 ? "+" : "";
        var amount = cents / 100m;
        var formatted = amount.ToString("N2", CultureInfo.InvariantCulture);

        return showCurrency ? $"{sign}€{formatted}" : $"{sign}{formatted}";
    }

    /// <summary>
    /// Formats cents to a currency string with color class for Bootstrap.
    /// Positive values return "text-success", negative return "text-danger", zero return "text-muted".
    /// </summary>
    /// <param name="cents">The amount in cents.</param>
    /// <returns>Bootstrap color class.</returns>
    public static string GetColorClass(long cents)
    {
        return cents switch
        {
            > 0 => "text-success",
            < 0 => "text-danger",
            _ => "text-muted"
        };
    }
}
