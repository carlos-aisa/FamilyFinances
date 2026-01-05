using Microsoft.AspNetCore.Components;
using FamilyFinances.Web.Auth;

namespace FamilyFinances.Web.Components.Base;

public abstract class ApiPageBase : ComponentBase
{
    [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected JwtAuthStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected IApiTokenStore TokenStore { get; set; } = default!;

    protected async Task ExecuteApiAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        var token = await TokenStore.WaitForTokenAsync(TimeSpan.FromSeconds(1), ct);
        if (string.IsNullOrWhiteSpace(token))
        {
            await RedirectToExpiredLoginAsync();
            return;
        }

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
        var token = await TokenStore.WaitForTokenAsync(TimeSpan.FromSeconds(1), ct);
        if (string.IsNullOrWhiteSpace(token))
        {
            await RedirectToExpiredLoginAsync();
            return default;
        }

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

    private async Task RedirectToExpiredLoginAsync()
    {
        // Clear HttpOnly cookie session in the Web host
        var http = HttpClientFactory.CreateClient();
        var url = new Uri(new Uri(Nav.BaseUri), "auth/session");
        await http.DeleteAsync(url);

        // Clear circuit token + notify UI
        TokenStore.Clear();
        await AuthProvider.RefreshAsync();

        Nav.NavigateTo("/login?reason=expired");
    }
}
