using FamilyFinances.Web.Auth;
using Microsoft.AspNetCore.Components;

namespace FamilyFinances.Web.Components.Base;

public abstract class ApiPageBase : ComponentBase
{
    [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected JwtAuthStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected IApiTokenStore TokenStore { get; set; } = default!;

    protected async Task ExecuteApiAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        if (!await EnsureTokenLoadedAsync(ct))
            return;

        try
        {
            await action(ct);
        }
        catch (UnauthorizedAccessException)
        {
            await RedirectToExpiredLoginAsync();
        }
    }

    protected async Task<T?> ExecuteApiAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        if (!await EnsureTokenLoadedAsync(ct))
            return default;

        try
        {
            return await action(ct);
        }
        catch (UnauthorizedAccessException)
        {
            await RedirectToExpiredLoginAsync();
            return default;
        }
    }

    private async Task<bool> EnsureTokenLoadedAsync(CancellationToken ct)
    {
        // On refresh (F5), a new circuit is created and the in-memory token is empty.
        // SessionBootstrapper will rehydrate it asynchronously from the HttpOnly cookie.
        // We must NOT treat "token not available yet" as an expired session.
        var token = await TokenStore.WaitForTokenAsync(TimeSpan.FromSeconds(3), ct);

        if (!string.IsNullOrWhiteSpace(token))
            return true;

        // Still no token: the user is not authenticated (or cookie doesn't exist).
        // Do not clear cookie here; just navigate to login.
        Nav.NavigateTo("/login");
        return false;
    }

    private async Task RedirectToExpiredLoginAsync()
    {
        // A 401 from the API means the token is invalid/expired.
        // Clear the HttpOnly cookie session in the Web host.
        var http = HttpClientFactory.CreateClient();
        var url = new Uri(new Uri(Nav.BaseUri), "auth/session");
        await http.DeleteAsync(url);

        // Clear circuit token + notify UI
        TokenStore.Clear();
        await AuthProvider.RefreshAsync();

        Nav.NavigateTo("/login?reason=expired");
    }
}
