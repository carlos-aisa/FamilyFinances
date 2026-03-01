using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Web.Components.Pages.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Settings;

public sealed class SettingsPageLanguageTests : WebTestContext
{
    [Fact]
    public void Authorized_User_Sees_Language_Selector_With_Two_Options_And_Initial_Culture()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var cut = RenderComponent<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            var selector = cut.Find("#settings-language-selector");
            var optionValues = selector.QuerySelectorAll("option")
                .Select(option => option.GetAttribute("value"))
                .ToArray();

            optionValues.Should().Equal("es-ES", "en-US");
            selector.GetAttribute("value").Should().Be("en-US");
            cut.Markup.Should().Contain("ff-settings-card");
        });
    }

    [Fact]
    public async Task Changing_Language_Persists_And_ForceReloads_Current_Route()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("es-ES");
        var setCultureCall = JSInterop.Setup<string>("cultureHelper.setCulture", invocation =>
            invocation.Arguments.Count == 1 &&
            string.Equals(invocation.Arguments[0]?.ToString(), "en-US", StringComparison.Ordinal))
            .SetResult("en-US");

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var currentUri = nav.Uri;

        var cut = RenderComponent<SettingsPage>();

        await cut.InvokeAsync(() => cut.Find("#settings-language-selector").Change("en-US"));

        cut.WaitForAssertion(() =>
        {
            setCultureCall.Invocations.Should().ContainSingle();
            nav.Uri.Should().Be(currentUri);
            nav.History.Should().NotBeEmpty();
            nav.History.First().Uri.Should().Be(currentUri);
            nav.History.First().Options.ForceLoad.Should().BeTrue();
        });
    }

    [Fact]
    public void Unsupported_Persisted_Culture_Falls_Back_To_Default_Value()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("fr-FR");

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("test-user");

        var cut = RenderComponent<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("#settings-language-selector")
                .GetAttribute("value")
                .Should().Be("es-ES");
        });
    }
}
