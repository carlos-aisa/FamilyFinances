using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace FamilyFinances.Web.Tests.Features.Localization;

public sealed class SharedResourceLocalizationTests : WebTestContext
{
    private static readonly string[] UiFullViewsReviewKeys =
    [
        "Dashboard_Analytics_Subtitle",
        "MonthlySummary_SelectedMonth",
        "Accounts_UpdatedAsOf",
        "QuickEntry_Guidance_Expense",
        "QuickEntry_Guidance_Income",
        "QuickEntry_Guidance_Transfer",
        "QuickEntry_Guidance_Refund",
        "QuickEntry_SearchAccountsPlaceholder",
        "Transactions_TablePayee",
        "Login_EmailPlaceholder",
        "ReportsIndex_AccountAnalysis_Title",
        "Reports_Balance",
        "Reports_BadgeYear",
        "Reports_BadgePeriod"
    ];

    private static readonly CultureInfo[] SupportedCultures =
    [
        CultureInfo.GetCultureInfo("es-ES"),
        CultureInfo.GetCultureInfo("en-US")
    ];

    public SharedResourceLocalizationTests()
    {
    }

    [Fact]
    public void SharedResource_Resolves_Nav_Home_Value()
    {
        typeof(SharedResource).FullName.Should().Be("FamilyFinances.Web.SharedResource");

        var resourceManager = new ResourceManager("FamilyFinances.Web.SharedResource", typeof(SharedResource).Assembly);
        resourceManager.GetString("Nav_Home", CultureInfo.GetCultureInfo("en-US")).Should().NotBeNullOrWhiteSpace();

        var localizer = Services.GetRequiredService<IStringLocalizer<SharedResource>>();

        localizer["Nav_Home"].Value.Should().NotBe("Nav_Home");
    }

    [Fact]
    public void SharedResource_Contains_UiReviewKeys_In_All_Supported_Cultures()
    {
        var resourceManager = new ResourceManager("FamilyFinances.Web.SharedResource", typeof(SharedResource).Assembly);

        foreach (var key in UiFullViewsReviewKeys)
        {
            foreach (var culture in SupportedCultures)
            {
                var value = resourceManager.GetString(key, culture);
                value.Should().NotBeNullOrWhiteSpace($"missing key {key} for culture {culture.Name}");
            }
        }
    }
}
