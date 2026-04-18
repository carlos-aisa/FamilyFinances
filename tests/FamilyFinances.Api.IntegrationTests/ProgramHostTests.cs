using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FamilyFinances.Api.IntegrationTests;

public sealed class ProgramHostTests
{
    [Fact]
    public async Task HealthEndpoint_ReturnsOk_InTestingEnvironment()
    {
        await using var factory = CreateFactory("Testing");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SwaggerEndpoint_IsNotExposed_InTestingEnvironment()
    {
        await using var factory = CreateFactory("Testing");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk_InDevelopmentEnvironment_WithHttpsBaseAddress()
    {
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment)
    {
        var baseFactory = TestClient.CreateFactoryWithFreshDb(out _);
        return baseFactory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
    }
}
