using System.Globalization;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class MoneyFormatterTests
{
    [Theory]
    [InlineData(123456, "es-ES", "1.234,56 \u20AC")]
    [InlineData(123456, "en-US", "\u20AC1,234.56")]
    [InlineData(-123456, "es-ES", "-1.234,56 \u20AC")]
    [InlineData(-123456, "en-US", "-\u20AC1,234.56")]
    [InlineData(0, "es-ES", "0,00 \u20AC")]
    [InlineData(0, "en-US", "\u20AC0.00")]
    public void FormatCents_WithCurrency_UsesRequestedCulture(long cents, string cultureName, string expected)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        var result = MoneyFormatter.FormatCents(cents, showCurrency: true, culture);

        NormalizeSpaces(result).Should().Be(expected);
    }

    [Theory]
    [InlineData(123456, "es-ES", "1.234,56")]
    [InlineData(123456, "en-US", "1,234.56")]
    [InlineData(-100, "es-ES", "-1,00")]
    [InlineData(-100, "en-US", "-1.00")]
    public void FormatCents_WithoutCurrency_UsesRequestedCulture(long cents, string cultureName, string expected)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        var result = MoneyFormatter.FormatCents(cents, showCurrency: false, culture);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "es-ES", "+1,00 \u20AC")]
    [InlineData(100, "en-US", "+\u20AC1.00")]
    [InlineData(0, "es-ES", "0,00 \u20AC")]
    [InlineData(0, "en-US", "\u20AC0.00")]
    [InlineData(-100, "es-ES", "-1,00 \u20AC")]
    [InlineData(-100, "en-US", "-\u20AC1.00")]
    public void FormatCentsWithSign_UsesRequestedCulture(long cents, string cultureName, string expected)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        var result = MoneyFormatter.FormatCentsWithSign(cents, showCurrency: true, culture);

        NormalizeSpaces(result).Should().Be(expected);
    }

    [Theory]
    [InlineData(1234.56, "es-ES", "1.234,56 \u20AC")]
    [InlineData(1234.56, "en-US", "\u20AC1,234.56")]
    [InlineData(-1234.56, "es-ES", "-1.234,56 \u20AC")]
    [InlineData(-1234.56, "en-US", "-\u20AC1,234.56")]
    public void FormatEuros_UsesRequestedCulture(decimal euros, string cultureName, string expected)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        var result = MoneyFormatter.FormatEuros(euros, showCurrency: true, culture);

        NormalizeSpaces(result).Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "text-success")]
    [InlineData(1, "text-success")]
    [InlineData(0, "text-muted")]
    [InlineData(-1, "text-danger")]
    [InlineData(-100, "text-danger")]
    public void GetColorClass_ReturnsCorrectBootstrapClass(long cents, string expected)
    {
        var result = MoneyFormatter.GetColorClass(cents);

        result.Should().Be(expected);
    }

    private static string NormalizeSpaces(string value)
        => value.Replace('\u00A0', ' ');
}

