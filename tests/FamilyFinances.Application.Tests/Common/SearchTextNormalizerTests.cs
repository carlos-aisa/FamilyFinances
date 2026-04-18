using FamilyFinances.Application.Common;
using FluentAssertions;

namespace FamilyFinances.Application.Tests.Common;

public sealed class SearchTextNormalizerTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void NormalizeForSearch_ReturnsEmpty_ForNullOrWhitespace(string? input, string expected)
    {
        var normalized = SearchTextNormalizer.NormalizeForSearch(input);
        normalized.Should().Be(expected);
    }

    [Theory]
    [InlineData("María", "maria")]
    [InlineData("  José  ", "jose")]
    [InlineData("ÁÉÍÓÚ", "aeiou")]
    [InlineData("Ñandú", "nandu")]
    [InlineData("CAFÉ", "cafe")]
    public void NormalizeForSearch_RemovesDiacritics_AndLowercases(string input, string expected)
    {
        var normalized = SearchTextNormalizer.NormalizeForSearch(input);
        normalized.Should().Be(expected);
    }

    [Theory]
    [InlineData("María", "maria")]
    [InlineData("José", "JOSE")]
    [InlineData("Camión", "camion")]
    public void NormalizeForSearch_IsSymmetric_ForAccentedAndNonAccentedText(string left, string right)
    {
        var leftNormalized = SearchTextNormalizer.NormalizeForSearch(left);
        var rightNormalized = SearchTextNormalizer.NormalizeForSearch(right);

        leftNormalized.Should().Be(rightNormalized);
    }
}
