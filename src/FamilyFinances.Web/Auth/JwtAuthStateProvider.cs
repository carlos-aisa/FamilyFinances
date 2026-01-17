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
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    /// <summary>
    /// Forces consumers (AuthorizeView/AuthorizeRouteView) to recompute the current auth state.
    /// Use this after the Web host sets or clears the HttpOnly cookie via /auth/session endpoints.
    /// </summary>
    public Task RefreshAsync()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }
}
