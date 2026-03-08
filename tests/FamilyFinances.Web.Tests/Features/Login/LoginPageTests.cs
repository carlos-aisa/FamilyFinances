using System.Collections.ObjectModel;
using Bunit;
using FamilyFinances.Web.Auth;
using FamilyFinances.Web.Components.Pages.Login;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace FamilyFinances.Web.Tests.Features.Login;

public sealed class LoginPageTests : WebTestContext
{
    [Fact]
    public void LoginPage_Prefills_Email_From_Last_Username_Without_Prefilling_Password()
    {
        var jsRuntime = new RecordingLoginJsRuntime
        {
            LastUsername = "last.user@familyfinances.local"
        };

        RegisterServices(jsRuntime, out _);

        var cut = RenderComponent<LoginPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[type='email']").GetAttribute("value").Should().Be("last.user@familyfinances.local");
            cut.Find("input[type='email']").GetAttribute("placeholder").Should().NotBeNullOrWhiteSpace();
            cut.Find("input[type='email']").GetAttribute("placeholder").Should().NotBe("admin@familyfinances.local");
            cut.Find("input[type='password']").GetAttribute("value").Should().BeEmpty();
        });

        jsRuntime.Invocations.Should().Contain(invocation => invocation.Identifier == "loginHelper.getLastUsername");
        jsRuntime.Invocations.Should().NotContain(invocation => invocation.Identifier == "loginHelper.setLastUsername");
    }

    [Fact]
    public void LoginPage_Stores_Only_Username_After_Successful_Login_And_Never_Persists_Password()
    {
        var jsRuntime = new RecordingLoginJsRuntime
        {
            LastUsername = null,
            ShouldLoginSucceed = true,
            AccessToken = "token-from-js"
        };

        RegisterServices(jsRuntime, out var tokenStore);

        var cut = RenderComponent<LoginPage>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[type='button']").HasAttribute("disabled").Should().BeFalse();
        });

        const string email = "new.user@familyfinances.local";
        const string password = "Secret123!";

        cut.Find("input[type='email']").Change(email);
        cut.Find("input[type='password']").Change(password);
        cut.Find("button[type='button']").Click();

        cut.WaitForAssertion(() =>
        {
            tokenStore.GetAccessToken().Should().Be("token-from-js");
            jsRuntime.StoredUsername.Should().Be(email);
            jsRuntime.LastSubmittedPassword.Should().Be(password);
        });

        var setUsernameCalls = jsRuntime.Invocations
            .Where(invocation => invocation.Identifier == "loginHelper.setLastUsername")
            .ToList();

        setUsernameCalls.Should().ContainSingle();
        setUsernameCalls[0].Arguments.Should().HaveCount(1);
        setUsernameCalls[0].Arguments[0]?.ToString().Should().Be(email);

        jsRuntime.Invocations.Should().NotContain(invocation =>
            invocation.Identifier.Contains("password", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(invocation.Identifier, "loginHelper.executeLogin", StringComparison.OrdinalIgnoreCase));

        jsRuntime.Invocations.Should().Contain(invocation => invocation.Identifier == "sessionHelper.markSessionActive");
    }

    private void RegisterServices(RecordingLoginJsRuntime jsRuntime, out TestTokenStore tokenStore)
    {
        tokenStore = new TestTokenStore();
        var authProvider = new JwtAuthStateProvider(tokenStore);

        Services.AddLogging();
        Services.AddSingleton<IJSRuntime>(jsRuntime);
        Services.AddSingleton<IApiTokenStore>(tokenStore);
        Services.AddSingleton(authProvider);
        Services.AddSingleton<AuthenticationStateProvider>(authProvider);
    }

    private sealed class RecordingLoginJsRuntime : IJSRuntime
    {
        private readonly List<JsInvocation> _invocations = [];

        public string? LastUsername { get; set; }
        public bool ShouldLoginSucceed { get; set; } = true;
        public string AccessToken { get; set; } = "test-token";
        public string? StoredUsername { get; private set; }
        public string? LastSubmittedPassword { get; private set; }
        public ReadOnlyCollection<JsInvocation> Invocations => _invocations.AsReadOnly();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            var copiedArgs = args?.ToArray() ?? Array.Empty<object?>();
            _invocations.Add(new JsInvocation(identifier, copiedArgs));

            if (identifier == "loginHelper.getLastUsername")
            {
                if (LastUsername is null)
                    return ValueTask.FromResult(default(TValue)!);

                return ValueTask.FromResult((TValue)(object)LastUsername);
            }

            if (identifier == "loginHelper.setLastUsername")
            {
                StoredUsername = copiedArgs.FirstOrDefault()?.ToString();
                return ValueTask.FromResult(default(TValue)!);
            }

            if (identifier == "sessionHelper.markSessionActive")
            {
                return ValueTask.FromResult(default(TValue)!);
            }

            if (identifier == "loginHelper.executeLogin")
            {
                LastSubmittedPassword = copiedArgs.Length > 1 ? copiedArgs[1]?.ToString() : null;
                var result = Activator.CreateInstance(typeof(TValue))
                    ?? throw new InvalidOperationException($"Unable to create JS result payload for {typeof(TValue).FullName}.");

                var successProperty = typeof(TValue).GetProperty("Success");
                var accessTokenProperty = typeof(TValue).GetProperty("AccessToken");
                var errorProperty = typeof(TValue).GetProperty("Error");

                successProperty?.SetValue(result, ShouldLoginSucceed);
                accessTokenProperty?.SetValue(result, ShouldLoginSucceed ? AccessToken : null);
                errorProperty?.SetValue(result, ShouldLoginSucceed ? null : "Login failed");

                return ValueTask.FromResult((TValue)result);
            }

            return ValueTask.FromResult(default(TValue)!);
        }
    }

    private sealed record JsInvocation(string Identifier, IReadOnlyList<object?> Arguments);

    private sealed class TestTokenStore : IApiTokenStore
    {
        private string? _token;

        public string? GetAccessToken() => _token;

        public void SetAccessToken(string accessToken) => _token = accessToken;

        public void Clear() => _token = null;

        public Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct) => Task.FromResult(_token);
    }
}
