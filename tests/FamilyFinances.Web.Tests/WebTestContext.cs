using Bunit;
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
}
