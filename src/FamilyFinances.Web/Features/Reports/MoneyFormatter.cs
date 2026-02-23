using System.Globalization;
using FamilyFinances.Domain.Common;

namespace FamilyFinances.Web.Features.Reports;

/// <summary>
/// Centralized money formatting for UI rendering.
/// </summary>
public static class MoneyFormatter
{
    private const string EuroSymbol = "\u20AC";

    /// <summary>
    /// Formats cents to localized euros with optional currency symbol.
    /// </summary>
    public static string FormatCents(long cents, bool showCurrency = true, CultureInfo? culture = null)
        => FormatMoneyCore(new Money(cents), showCurrency, forceSignForPositive: false, culture);

    /// <summary>
    /// Formats cents to localized euros and always prefixes '+' for positive values.
    /// </summary>
    public static string FormatCentsWithSign(long cents, bool showCurrency = true, CultureInfo? culture = null)
        => FormatMoneyCore(new Money(cents), showCurrency, forceSignForPositive: true, culture);

    /// <summary>
    /// Formats euros to localized display with optional currency symbol.
    /// </summary>
    public static string FormatEuros(decimal amount, bool showCurrency = true, CultureInfo? culture = null)
        => FormatMoneyCore(Money.FromEuros(amount), showCurrency, forceSignForPositive: false, culture);

    /// <summary>
    /// Formats euros to localized display and always prefixes '+' for positive values.
    /// </summary>
    public static string FormatEurosWithSign(decimal amount, bool showCurrency = true, CultureInfo? culture = null)
        => FormatMoneyCore(Money.FromEuros(amount), showCurrency, forceSignForPositive: true, culture);

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

    private static string FormatMoneyCore(Money money, bool showCurrency, bool forceSignForPositive, CultureInfo? culture)
    {
        var displayCulture = ResolveCulture(culture);
        var formatInfo = (NumberFormatInfo)displayCulture.NumberFormat.Clone();
        formatInfo.CurrencySymbol = EuroSymbol;

        var euros = money.ToEuros();
        var magnitude = Math.Abs(euros).ToString(showCurrency ? "C2" : "N2", formatInfo);
        var sign = euros < 0 ? "-" : (forceSignForPositive && euros > 0 ? "+" : string.Empty);

        return $"{sign}{magnitude}";
    }

    private static CultureInfo ResolveCulture(CultureInfo? culture)
    {
        return culture
            ?? CultureInfo.CurrentCulture
            ?? CultureInfo.GetCultureInfo("es-ES");
    }
}
