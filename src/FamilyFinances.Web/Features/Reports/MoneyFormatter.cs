using System.Globalization;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Web.Features.Reports;

/// <summary>
/// Centralized money formatting for UI rendering.
/// </summary>
public static class MoneyFormatter
{
    private const string EuroSymbol = "\u20AC";
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("es-ES");

    /// <summary>
    /// Formats cents to localized euros with optional currency symbol.
    /// Example: 123456 -> "1.234,56 EUR".
    /// </summary>
    public static string FormatCents(long cents, bool showCurrency = true)
        => FormatMoneyCore(new Money(cents), showCurrency, forceSignForPositive: false);

    /// <summary>
    /// Formats cents to localized euros and always prefixes '+' for positive values.
    /// Example: 123456 -> "+1.234,56 EUR".
    /// </summary>
    public static string FormatCentsWithSign(long cents, bool showCurrency = true)
        => FormatMoneyCore(new Money(cents), showCurrency, forceSignForPositive: true);

    /// <summary>
    /// Formats euros to localized display with optional currency symbol.
    /// </summary>
    public static string FormatEuros(decimal amount, bool showCurrency = true)
        => FormatMoneyCore(Money.FromEuros(amount), showCurrency, forceSignForPositive: false);

    /// <summary>
    /// Formats euros to localized display and always prefixes '+' for positive values.
    /// </summary>
    public static string FormatEurosWithSign(decimal amount, bool showCurrency = true)
        => FormatMoneyCore(Money.FromEuros(amount), showCurrency, forceSignForPositive: true);

    /// <summary>
    /// Returns Bootstrap color class from sign.
    /// </summary>
    public static string GetColorClass(long cents)
    {
        return cents switch
        {
            > 0 => "text-success",
            < 0 => "text-danger",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// Returns Bootstrap color class from decimal euro amount.
    /// </summary>
    public static string GetColorClass(decimal amount)
        => GetColorClass(Money.FromEuros(amount).Cents);

    private static string FormatMoneyCore(Money money, bool showCurrency, bool forceSignForPositive)
    {
        var euros = money.ToEuros();
        var absoluteText = Math.Abs(euros).ToString("N2", DisplayCulture);
        var sign = euros < 0 ? "-" : (forceSignForPositive && euros > 0 ? "+" : string.Empty);
        var value = $"{sign}{absoluteText}";

        return showCurrency ? $"{value}{EuroSymbol}" : value;
    }
}
