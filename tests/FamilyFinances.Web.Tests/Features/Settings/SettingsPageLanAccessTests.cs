using Bunit;
using Bunit.TestDoubles;
using FamilyFinances.Web.Components.Pages.Settings;
using FamilyFinances.Web.Features.HostOps;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Settings;

public sealed class SettingsPageLanAccessTests : WebTestContext
{
    [Fact]
    public void Admin_User_Sees_Network_Access_Panel()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Network Access");
            cut.Find("[data-testid='settings-lan-toggle']");
            cut.Find("[data-testid='settings-network-apply']");
            cut.Find("[data-testid='settings-network-regenerate']");
        });
    }

    [Fact]
    public void Non_Admin_User_Does_Not_See_Network_Access_Panel()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("reader-user");
        authContext.SetRoles("Reader");

        var cut = RenderComponent<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain("Network Access");
            cut.FindAll("[data-testid='settings-lan-toggle']").Should().BeEmpty();
        });
    }

    [Fact]
    public void Admin_User_With_AccessLimited_Status_Sees_Admin_Permissions_Message_And_Disabled_Actions()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        JSInterop.Setup<string>("themeHelper.getTheme").SetResult("dark");
        JSInterop.Setup<string>("cultureHelper.getCulture").SetResult("en-US");

        Services.AddSingleton<ILanHostOperationsService>(new AccessLimitedLanHostOperationsService());

        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("admin-user");
        authContext.SetRoles("Admin");

        var cut = RenderComponent<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='settings-network-message']")
                .TextContent.Should().Contain("administrator permissions");
            cut.Find("[data-testid='settings-network-apply']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("[data-testid='settings-network-regenerate']").HasAttribute("disabled").Should().BeTrue();
        });
    }

    private sealed class AccessLimitedLanHostOperationsService : ILanHostOperationsService
    {
        public Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new LanAccessStatus(
                Enabled: false,
                HttpsPort: 5443,
                HostName: Environment.MachineName,
                CertificateThumb: null,
                CertificateSubject: null,
                FirewallRuleName: "FamilyFinances.Web.LAN.HTTPS",
                FirewallEnabled: false,
                AccessLimited: true,
                Diagnostic: "IIS status unavailable: elevated process required."));
        }

        public Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
        {
            return Task.FromResult(new LanOperationResult(false, "LAN access changes require administrator permissions on this machine."));
        }

        public Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default)
        {
            return Task.FromResult(new LanOperationResult(false, "LAN access changes require administrator permissions on this machine."));
        }
    }
}
