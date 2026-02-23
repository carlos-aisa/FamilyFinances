using FamilyFinances.Web.Features.Localization;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Localization;

public sealed class WebLocalizationOptionsTests
{
    [Fact]
    public void Normalize_ReturnsDefault_WhenCultureIsMissing()
    {
        WebLocalizationOptions.Normalize(null).Should().Be(WebLocalizationOptions.DefaultCulture);
        WebLocalizationOptions.Normalize(string.Empty).Should().Be(WebLocalizationOptions.DefaultCulture);
        WebLocalizationOptions.Normalize(" ").Should().Be(WebLocalizationOptions.DefaultCulture);
    }

    [Theory]
    [InlineData("es-ES", "es-ES")]
    [InlineData("ES-es", "es-ES")]
    [InlineData("en-US", "en-US")]
    [InlineData("EN-us", "en-US")]
    public void Normalize_ReturnsCanonicalSupportedCulture(string input, string expected)
    {
        WebLocalizationOptions.Normalize(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("es")]
    [InlineData("en")]
    public void Normalize_FallsBackToDefault_ForUnsupportedCultures(string input)
    {
        WebLocalizationOptions.Normalize(input).Should().Be(WebLocalizationOptions.DefaultCulture);
    }
}
