using FamilyFinances.Web.Features.Reports;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Reports;

public sealed class DateHelperTests
{
    [Fact]
    public void GetCurrentMonthStart_ReturnsFirstDayOfCurrentMonth()
    {
        // Act
        var result = DateHelper.GetCurrentMonthStart();

        // Assert
        result.Day.Should().Be(1);
        // Note: Exact year/month will vary based on when test runs,
        // but we can verify it's a valid date and the first day of a month
        result.Should().BeAfter(new DateOnly(2020, 1, 1));
    }

    [Fact]
    public void GetCurrentMonthEnd_ReturnsFirstDayOfNextMonth()
    {
        // Arrange
        var start = DateHelper.GetCurrentMonthStart();

        // Act
        var result = DateHelper.GetCurrentMonthEnd();

        // Assert
        result.Should().Be(start.AddMonths(1));
        result.Day.Should().Be(1);
    }

    [Fact]
    public void GetCurrentYear_ReturnsValidYear()
    {
        // Act
        var result = DateHelper.GetCurrentYear();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(2024);
        result.Should().BeLessThanOrEqualTo(2100);
    }

    [Fact]
    public void GetCurrentMonth_ReturnsValidMonth()
    {
        // Act
        var result = DateHelper.GetCurrentMonth();

        // Assert
        result.Should().BeInRange(1, 12);
    }

    [Theory]
    [InlineData(1, "January")]
    [InlineData(2, "February")]
    [InlineData(3, "March")]
    [InlineData(4, "April")]
    [InlineData(5, "May")]
    [InlineData(6, "June")]
    [InlineData(7, "July")]
    [InlineData(8, "August")]
    [InlineData(9, "September")]
    [InlineData(10, "October")]
    [InlineData(11, "November")]
    [InlineData(12, "December")]
    public void GetMonthName_ReturnsCorrectMonthName(int month, string expected)
    {
        // Act
        var result = DateHelper.GetMonthName(month);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(2026, 1, 2026, 1, 1)]
    [InlineData(2026, 12, 2026, 12, 1)]
    [InlineData(2025, 6, 2025, 6, 1)]
    public void GetMonthStart_ReturnsFirstDayOfGivenMonth(int year, int month, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Act
        var result = DateHelper.GetMonthStart(year, month);

        // Assert
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
        // Act
        var result = DateHelper.GetMonthEnd(year, month);

        // Assert
        result.Year.Should().Be(expectedYear);
        result.Month.Should().Be(expectedMonth);
        result.Day.Should().Be(expectedDay);
    }
}
