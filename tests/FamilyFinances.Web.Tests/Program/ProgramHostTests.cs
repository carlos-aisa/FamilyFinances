using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.ProgramHost;

public sealed class ProgramHostTests
{
    [Fact]
    public async Task GetSession_ReturnsNoContent_WhenUnauthenticated()
    {
        using var factory = CreateFactory("Testing");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DevelopmentHost_AllowsAuthSessionRoute_WithOrWithoutHttpsRedirect()
    {
        using var factory = CreateFactory("Development");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/auth/session");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.TemporaryRedirect,
            HttpStatusCode.PermanentRedirect);
    }

    [Fact]
    public void FamilyFinancesApiClient_UsesConfiguredDefaultBaseUrl_WhenApiBaseUrlNotOverridden()
    {
        using var factory = CreateFactory("Testing");

        var client = factory.Services.GetRequiredService<IHttpClientFactory>().CreateClient("FamilyFinancesApi");

        client.BaseAddress.Should().NotBeNull();
        client.BaseAddress!.Scheme.Should().Be(Uri.UriSchemeHttp);
        client.BaseAddress.Host.Should().BeOneOf("localhost", "127.0.0.1");
        client.BaseAddress.Port.Should().Be(5084);
    }

    [Fact]
    public void FamilyFinancesApiClient_InDevelopment_UsesExpectedLocalBaseUrl()
    {
        using var factory = CreateFactory("Development");

        var client = factory.Services.GetRequiredService<IHttpClientFactory>().CreateClient("FamilyFinancesApi");

        client.BaseAddress.Should().NotBeNull();
        client.BaseAddress!.Host.Should().BeOneOf("localhost", "127.0.0.1");
        client.BaseAddress.Scheme.Should().BeOneOf(Uri.UriSchemeHttp, Uri.UriSchemeHttps);
        client.BaseAddress.Port.Should().BeOneOf(5084, 5184, 7349);
    }

    [Fact]
    public void FamilyFinancesApiClient_Throws_WhenApiBaseUrlIsInvalid()
    {
        using var factory = CreateFactory(
            "Testing",
            new Dictionary<string, string?>
            {
                ["Api:BaseUrl"] = "invalid-base-url"
            });

        var act = () => factory.Services.GetRequiredService<IHttpClientFactory>().CreateClient("FamilyFinancesApi");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid Api:BaseUrl value*");
    }

    private static WebApplicationFactory<global::Program> CreateFactory(
        string environment,
        IReadOnlyDictionary<string, string?>? additionalConfiguration = null)
    {
        return new WebApplicationFactory<global::Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);

                if (additionalConfiguration is null)
                    return;

                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(additionalConfiguration);
                });
            });
    }
}
