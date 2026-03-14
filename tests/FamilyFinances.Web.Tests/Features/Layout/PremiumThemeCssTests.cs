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

    [Fact]
    public void UiTokens_File_Exists_And_Defines_Core_Tokens()
    {
        var tokensPath = GetUiTokensPath();
        var css = File.ReadAllText(tokensPath);

        css.Should().Contain("--ff-font-heading");
        css.Should().Contain("--ff-accent-primary");
        css.Should().Contain("--ff-chart-grid-color");
        css.Should().Contain("--ff-chart-cutoff-line");
        css.Should().Contain("--ff-button-radius");
    }

    [Fact]
    public void AppHost_Loads_Tokens_Before_App_And_Premium_Styles()
    {
        var appRazor = File.ReadAllText(GetAppHostPath());

        var uiTokensIndex = appRazor.IndexOf("css/ui-tokens.css", StringComparison.Ordinal);
        var appCssIndex = appRazor.IndexOf("app.css", StringComparison.Ordinal);
        var premiumIndex = appRazor.IndexOf("css/premium-theme.css", StringComparison.Ordinal);

        uiTokensIndex.Should().BeGreaterThanOrEqualTo(0);
        appCssIndex.Should().BeGreaterThan(uiTokensIndex);
        premiumIndex.Should().BeGreaterThan(appCssIndex);
    }

    [Fact]
    public void Shared_AppCss_ImportChain_Loads_Tokens_First()
    {
        var appCss = File.ReadAllText(GetSharedAppCssPath());
        var tokensIndex = appCss.IndexOf("@import url(\"css/ui-tokens.css\")", StringComparison.Ordinal);
        var sharedCssIndex = appCss.IndexOf("@import url(\"css/app.css\")", StringComparison.Ordinal);

        tokensIndex.Should().BeGreaterThanOrEqualTo(0);
        sharedCssIndex.Should().BeGreaterThan(tokensIndex);
    }

    private static string GetPremiumThemePath()
    {
        var path = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "FamilyFinances.Web",
            "wwwroot",
            "css",
            "premium-theme.css");

        File.Exists(path).Should().BeTrue("premium-theme.css must exist");
        return path;
    }

    private static string GetUiTokensPath()
    {
        var path = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "FamilyFinances.Web",
            "wwwroot",
            "css",
            "ui-tokens.css");

        File.Exists(path).Should().BeTrue("ui-tokens.css must exist");
        return path;
    }

    private static string GetAppHostPath()
    {
        var path = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "FamilyFinances.Web",
            "Components",
            "App.razor");

        File.Exists(path).Should().BeTrue("App.razor must exist");
        return path;
    }

    private static string GetSharedAppCssPath()
    {
        var path = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "FamilyFinances.Web",
            "wwwroot",
            "app.css");

        File.Exists(path).Should().BeTrue("wwwroot/app.css must exist");
        return path;
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FamilyFinances.sln")))
        {
            current = current.Parent;
        }

        current.Should().NotBeNull("tests should execute from within the repository tree");
        return current!.FullName;
    }
}
