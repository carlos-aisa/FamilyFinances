using System.Globalization;

namespace FamilyFinances.Web.Features.Localization;

public static class WebLocalizationOptions
{
    public const string DefaultCulture = "es-ES";

    public static readonly string[] SupportedCultureNames =
    [
        "es-ES",
        "en-US"
    ];

    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } =
        SupportedCultureNames.Select(CultureInfo.GetCultureInfo).ToArray();

    public static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return DefaultCulture;
        }

        var match = SupportedCultureNames
            .FirstOrDefault(x => string.Equals(x, culture, StringComparison.OrdinalIgnoreCase));

        return match ?? DefaultCulture;
    }
}
