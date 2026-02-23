using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace FamilyFinances.Web.Tests.Features.Localization;

public sealed class SharedResourceLocalizationTests : WebTestContext
{
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
}
