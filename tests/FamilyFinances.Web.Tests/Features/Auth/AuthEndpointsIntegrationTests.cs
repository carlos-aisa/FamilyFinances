using System.Net;
using System.Net.Http.Json;
using FamilyFinances.Web.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinances.Web.Tests.Features.Auth;

public sealed class AuthEndpointsIntegrationTests
{
    [Fact]
    public async Task Login_ReturnsBadRequest_WhenEmailOrPasswordIsMissing()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/auth/session", new { email = "", password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>();
        payload.Should().NotBeNull();
        payload!.Error.Should().Be("Login failed");
    }

    [Fact]
    public async Task Login_PropagatesClientAuthenticationErrorCodes()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/auth/session", new LoginRequest("user@familyfinances.local", "invalid"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>();
        payload.Should().NotBeNull();
        payload!.Error.Should().Be("Login failed");
    }

    [Fact]
    public async Task Login_OnSuccess_ReturnsTokenAndSetsHttpOnlyCookie()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new LoginResponse("token-123"))
            }));
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/auth/session", new LoginRequest("user@familyfinances.local", "valid-pass"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AccessTokenPayload>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().Be("token-123");
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var cookieHeaders = cookies!.ToArray();
        cookieHeaders.Should().ContainSingle();
        cookieHeaders[0].Should().Contain("ff_access_token=token-123");
        cookieHeaders[0].ToLowerInvariant().Should().Contain("httponly");
    }

    [Fact]
    public async Task GetSession_ReturnsNoContent_WhenCookieIsMissing()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = app.GetTestClient();

        var response = await client.GetAsync("/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetSession_ReturnsToken_WhenCookieExists()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("Cookie", "ff_access_token=session-token");

        var response = await client.GetAsync("/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AccessTokenPayload>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().Be("session-token");
    }

    [Fact]
    public async Task Logout_DeletesSessionCookie()
    {
        await using var app = await CreateTestAppAsync(new StubApiResponder(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = app.GetTestClient();

        var response = await client.DeleteAsync("/auth/session");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Single().Should().Contain("ff_access_token=");
    }

    private static async Task<WebApplication> CreateTestAppAsync(StubApiResponder responder)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(responder));

        var app = builder.Build();
        app.MapAuthEndpoints();
        await app.StartAsync();
        return app;
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(StubApiResponder responder)
        {
            _client = new HttpClient(new StubHttpMessageHandler(responder))
            {
                BaseAddress = new Uri("http://api.local/")
            };
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly StubApiResponder _responder;

        public StubHttpMessageHandler(StubApiResponder responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder.BuildResponse(request));
        }
    }

    private sealed class StubApiResponder
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

        public StubApiResponder(Func<HttpRequestMessage, HttpResponseMessage> factory)
        {
            _factory = factory;
        }

        public HttpResponseMessage BuildResponse(HttpRequestMessage request) => _factory(request);
    }

    private sealed record ErrorPayload(string Error);
    private sealed record AccessTokenPayload(string AccessToken);
}
