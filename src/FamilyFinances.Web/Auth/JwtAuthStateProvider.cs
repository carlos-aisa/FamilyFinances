using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FamilyFinances.Web.Auth;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    private readonly IApiTokenStore _tokenStore;

    public JwtAuthStateProvider(IApiTokenStore tokenStore)
        => _tokenStore = tokenStore;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokenStore.GetAccessToken();
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(new AuthenticationState(Anonymous));

        var claims = JwtParser.ParseClaimsFromJwt(token);

        // "jwt" is the auth type label; any non-empty string works
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    public void MarkUserAsAuthenticated(string accessToken)
    {
        _tokenStore.SetAccessToken(accessToken);

        var claims = JwtParser.ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        _tokenStore.Clear();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }
}
