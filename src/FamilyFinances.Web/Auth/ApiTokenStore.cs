namespace FamilyFinances.Web.Auth;

public sealed class ApiTokenStore : IApiTokenStore
{
    private string? _token;

    public string? GetAccessToken() => _token;

    public void SetAccessToken(string accessToken)
    {
        _token = accessToken;
    }

    public void Clear()
    {
        _token = null;
    }

    public Task TryLoadFromSessionAsync()
    {
        // No-op: tokens are loaded via SessionBootstrapper from /auth/session endpoint
        return Task.CompletedTask;
    }
}
