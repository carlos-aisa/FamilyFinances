using Bunit;
using FamilyFinances.Web.Features.HostOps;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

public abstract class WebTestContext : TestContext
{
    protected WebTestContext()
    {
        Services.AddLocalization();

        // Keep tests deterministic regardless of machine/OS locale.
        var defaultCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentCulture = defaultCulture;
        CultureInfo.CurrentUICulture = defaultCulture;

        Services.AddSingleton<ILanHostOperationsService, StubLanHostOperationsService>();
    }

    protected static IDisposable UseCulture(string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var selectedCulture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = selectedCulture;
        CultureInfo.CurrentUICulture = selectedCulture;
        return new CultureOverride(originalCulture, originalUiCulture);
    }

    private sealed class CultureOverride : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUiCulture;

        public CultureOverride(CultureInfo originalCulture, CultureInfo originalUiCulture)
        {
            _originalCulture = originalCulture;
            _originalUiCulture = originalUiCulture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }

    private sealed class StubLanHostOperationsService : ILanHostOperationsService
    {
        private LanAccessStatus _status = new(
            Enabled: false,
            HttpsPort: 5443,
            HostName: Environment.MachineName,
            CertificateThumb: null,
            CertificateSubject: null,
            FirewallRuleName: "FamilyFinances.Web.LAN.HTTPS",
            FirewallEnabled: false);

        public Task<LanAccessStatus> GetStatusAsync(CancellationToken ct = default) => Task.FromResult(_status);

        public Task<LanOperationResult> ApplyAsync(LanAccessRequest request, string actor, CancellationToken ct = default)
        {
            _status = _status with
            {
                Enabled = request.Enabled,
                HttpsPort = request.HttpsPort,
                HostName = string.IsNullOrWhiteSpace(request.HostName) ? Environment.MachineName : request.HostName,
                CertificateThumb = request.RegenerateCertificate ? "REGENERATED" : _status.CertificateThumb
            };

            return Task.FromResult(new LanOperationResult(true, "ok", _status));
        }

        public Task<LanOperationResult> RegenerateCertificateAsync(int httpsPort, string? hostName, string actor, CancellationToken ct = default)
        {
            _status = _status with
            {
                Enabled = true,
                HttpsPort = httpsPort,
                HostName = string.IsNullOrWhiteSpace(hostName) ? Environment.MachineName : hostName,
                CertificateThumb = "REGENERATED"
            };

            return Task.FromResult(new LanOperationResult(true, "ok", _status));
        }
    }
}
