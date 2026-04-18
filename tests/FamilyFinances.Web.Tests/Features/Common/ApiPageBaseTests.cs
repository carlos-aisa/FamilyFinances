using System.Net;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Base;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FamilyFinances.Web.Tests.Features.Common;

public sealed class ApiPageBaseTests
{
    [Fact]
    public async Task ExecuteApiAsync_InvokesAction_WhenTokenIsAvailable()
    {
        var tokenStore = new FakeApiTokenStore(waitToken: "token-123");
        var (_, page, _) = CreateSut(tokenStore);
        var called = false;

        await page.RunAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteApiAsync_NavigatesToLogin_WhenTokenCannotBeLoaded()
    {
        var tokenStore = new FakeApiTokenStore(waitToken: null);
        var (nav, page, _) = CreateSut(tokenStore);
        var called = false;

        await page.RunAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        called.Should().BeFalse();
        nav.Uri.Should().Be("http://localhost/login");
    }

    [Fact]
    public async Task ExecuteApiAsync_RedirectsToExpiredLogin_WhenActionThrowsUnauthorized()
    {
        var tokenStore = new FakeApiTokenStore(waitToken: "token-123", accessToken: "token-123");
        var (nav, page, calls) = CreateSut(tokenStore);
        var refreshed = false;
        page.PublicAuthProvider.AuthenticationStateChanged += _ => refreshed = true;

        await page.RunAsync(_ => throw new UnauthorizedAccessException("expired"));

        calls.Should().ContainSingle();
        calls[0].Method.Should().Be(HttpMethod.Delete);
        calls[0].RequestUri!.ToString().Should().Be("http://localhost/auth/session");
        tokenStore.ClearCalls.Should().Be(1);
        refreshed.Should().BeTrue();
        nav.Uri.Should().Be("http://localhost/login?reason=expired");
    }

    [Fact]
    public async Task ExecuteApiAsyncOfT_ReturnsValue_WhenSuccessful()
    {
        var tokenStore = new FakeApiTokenStore(waitToken: "token-123");
        var (_, page, _) = CreateSut(tokenStore);

        var result = await page.RunAsync(_ => Task.FromResult("ok"));

        result.Should().Be("ok");
    }

    [Fact]
    public async Task ExecuteApiAsyncOfT_ReturnsDefault_AndRedirects_WhenActionThrowsUnauthorized()
    {
        var tokenStore = new FakeApiTokenStore(waitToken: "token-123", accessToken: "token-123");
        var (nav, page, calls) = CreateSut(tokenStore);

        var result = await page.RunAsync<string>(_ => throw new UnauthorizedAccessException("expired"));

        result.Should().BeNull();
        calls.Should().ContainSingle();
        nav.Uri.Should().Be("http://localhost/login?reason=expired");
        tokenStore.ClearCalls.Should().Be(1);
    }

    private static (TestNavigationManager Nav, TestApiPage Page, IReadOnlyList<HttpRequestMessage> Calls) CreateSut(
        FakeApiTokenStore tokenStore)
    {
        var calls = new List<HttpRequestMessage>();
        var handler = new RecordingMessageHandler(calls);
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var clientFactory = new StubHttpClientFactory(client);
        var nav = new TestNavigationManager();
        var authProvider = new JwtAuthStateProvider(tokenStore);
        var page = new TestApiPage(clientFactory, nav, authProvider, tokenStore);
        return (nav, page, calls);
    }

    private sealed class TestApiPage : ApiPageBase
    {
        public TestApiPage(
            IHttpClientFactory clientFactory,
            NavigationManager navigationManager,
            JwtAuthStateProvider authProvider,
            IApiTokenStore tokenStore)
        {
            HttpClientFactory = clientFactory;
            Nav = navigationManager;
            AuthProvider = authProvider;
            TokenStore = tokenStore;
        }

        public JwtAuthStateProvider PublicAuthProvider => base.AuthProvider;

        public Task RunAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
            => ExecuteApiAsync(action, ct);

        public Task<T?> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
            => ExecuteApiAsync(action, ct);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class RecordingMessageHandler : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _calls;

        public RecordingMessageHandler(List<HttpRequestMessage> calls)
        {
            _calls = calls;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _calls.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = ToAbsoluteUri(uri).ToString();
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
            Uri = ToAbsoluteUri(uri).ToString();
        }
    }

    private sealed class FakeApiTokenStore : IApiTokenStore
    {
        private readonly string? _waitToken;
        private string? _accessToken;

        public FakeApiTokenStore(string? waitToken, string? accessToken = null)
        {
            _waitToken = waitToken;
            _accessToken = accessToken;
        }

        public int ClearCalls { get; private set; }

        public string? GetAccessToken() => _accessToken;

        public void SetAccessToken(string accessToken) => _accessToken = accessToken;

        public void Clear()
        {
            ClearCalls++;
            _accessToken = null;
        }

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_waitToken);
    }
}
