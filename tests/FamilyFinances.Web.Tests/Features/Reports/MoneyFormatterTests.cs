using FamilyFinances.Web.Features.Reports;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class MoneyFormatterTests
{
    [Theory]
    [InlineData(0, "€0.00")]
    [InlineData(100, "€1.00")]
    [InlineData(1234, "€12.34")]
    [InlineData(123456, "€1,234.56")]
    [InlineData(100000000, "€1,000,000.00")]
    [InlineData(-100, "€-1.00")]
    [InlineData(-123456, "€-1,234.56")]
    public void FormatCents_WithCurrency_ReturnsFormattedString(long cents, string expected)
    {
        // Act
        var result = MoneyFormatter.FormatCents(cents, showCurrency: true);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "0.00")]
    [InlineData(100, "1.00")]
    [InlineData(123456, "1,234.56")]
    [InlineData(-100, "-1.00")]
    public void FormatCents_WithoutCurrency_ReturnsFormattedString(long cents, string expected)
    {
        // Act
        var result = MoneyFormatter.FormatCents(cents, showCurrency: false);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "+€1.00")]
    [InlineData(123456, "+€1,234.56")]
    [InlineData(0, "€0.00")]
    [InlineData(-100, "€-1.00")]
    [InlineData(-123456, "€-1,234.56")]
    public void FormatCentsWithSign_WithCurrency_ReturnsFormattedStringWithSign(long cents, string expected)
    {
        // Act
        var result = MoneyFormatter.FormatCentsWithSign(cents, showCurrency: true);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "+1.00")]
    [InlineData(0, "0.00")]
    [InlineData(-100, "-1.00")]
    public void FormatCentsWithSign_WithoutCurrency_ReturnsFormattedStringWithSign(long cents, string expected)
    {
        // Act
        var result = MoneyFormatter.FormatCentsWithSign(cents, showCurrency: false);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "text-success")]
    [InlineData(1, "text-success")]
    [InlineData(0, "text-muted")]
    [InlineData(-1, "text-danger")]
    [InlineData(-100, "text-danger")]
    public void GetColorClass_ReturnsCorrectBootstrapClass(long cents, string expected)
    {
        // Act
        var result = MoneyFormatter.GetColorClass(cents);

        // Assert
        result.Should().Be(expected);
    }
}
