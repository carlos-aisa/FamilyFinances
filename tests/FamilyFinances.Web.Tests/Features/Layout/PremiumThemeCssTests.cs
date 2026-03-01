using System.IO;
using FluentAssertions;

namespace FamilyFinances.Web.Tests.Features.Layout;

public sealed class PremiumThemeCssTests
{
    [Fact]
    public void PremiumTheme_Defines_ReducedMotion_Guard_For_RevealAnimations()
    {
        var css = File.ReadAllText(GetPremiumThemePath());

        css.Should().Contain("@keyframes ffReveal");
        css.Should().Contain("@media (prefers-reduced-motion: reduce)");
        css.Should().Contain("animation: none;");
        css.Should().Contain("transform: none;");
    }

    private static string GetPremiumThemePath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FamilyFinances.sln")))
        {
            current = current.Parent;
        }

        current.Should().NotBeNull("tests should execute from within the repository tree");

        var path = Path.Combine(
            current!.FullName,
            "src",
            "FamilyFinances.Web",
            "wwwroot",
            "css",
            "premium-theme.css");

        File.Exists(path).Should().BeTrue("premium-theme.css must exist");
        return path;
    }
}
