using System.Globalization;
using FamilyFinances.Web.Features.Reports;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class DateHelperTests
{
    [Fact]
    public void GetCurrentMonthStart_ReturnsFirstDayOfCurrentMonth()
    {
        var result = DateHelper.GetCurrentMonthStart();

        result.Day.Should().Be(1);
        result.Should().BeAfter(new DateOnly(2020, 1, 1));
    }

    [Fact]
    public void GetCurrentMonthEnd_ReturnsFirstDayOfNextMonth()
    {
        var start = DateHelper.GetCurrentMonthStart();

        var result = DateHelper.GetCurrentMonthEnd();

        result.Should().Be(start.AddMonths(1));
        result.Day.Should().Be(1);
    }

    [Fact]
    public void GetCurrentYear_ReturnsValidYear()
    {
        var result = DateHelper.GetCurrentYear();

        result.Should().BeGreaterThanOrEqualTo(2024);
        result.Should().BeLessThanOrEqualTo(2100);
    }

    [Fact]
    public void GetCurrentMonth_ReturnsValidMonth()
    {
        var result = DateHelper.GetCurrentMonth();

        result.Should().BeInRange(1, 12);
    }

    [Theory]
    [InlineData(1, "January", "en-US")]
    [InlineData(2, "February", "en-US")]
    [InlineData(3, "March", "en-US")]
    [InlineData(1, "enero", "es-ES")]
    [InlineData(2, "febrero", "es-ES")]
    [InlineData(3, "marzo", "es-ES")]
    public void GetMonthName_UsesSpecifiedCulture(int month, string expected, string culture)
    {
        var result = DateHelper.GetMonthName(month, CultureInfo.GetCultureInfo(culture));

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("en-US", "January")]
    [InlineData("es-ES", "enero")]
    public void GetMonthName_UsesCurrentCulture_WhenCultureNotProvided(string cultureName, string expected)
    {
        var original = CultureInfo.CurrentCulture;
        var originalUi = CultureInfo.CurrentUICulture;
        try
        {
            var selected = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = selected;
            CultureInfo.CurrentUICulture = selected;

            var result = DateHelper.GetMonthName(1);

            result.Should().Be(expected);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
            CultureInfo.CurrentUICulture = originalUi;
        }
    }

    [Theory]
    [InlineData(2026, 1, 2026, 1, 1)]
    [InlineData(2026, 12, 2026, 12, 1)]
    [InlineData(2025, 6, 2025, 6, 1)]
    public void GetMonthStart_ReturnsFirstDayOfGivenMonth(int year, int month, int expectedYear, int expectedMonth, int expectedDay)
    {
        var result = DateHelper.GetMonthStart(year, month);

        result.Year.Should().Be(expectedYear);
        result.Month.Should().Be(expectedMonth);
        result.Day.Should().Be(expectedDay);
    }

    [Theory]
    [InlineData(2026, 1, 2026, 2, 1)]
    [InlineData(2026, 12, 2027, 1, 1)]
    [InlineData(2025, 6, 2025, 7, 1)]
    public void GetMonthEnd_ReturnsFirstDayOfNextMonth(int year, int month, int expectedYear, int expectedMonth, int expectedDay)
    {
        var result = DateHelper.GetMonthEnd(year, month);

        result.Year.Should().Be(expectedYear);
        result.Month.Should().Be(expectedMonth);
        result.Day.Should().Be(expectedDay);
    }
}
