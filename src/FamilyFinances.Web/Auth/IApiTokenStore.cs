namespace FamilyFinances.Web.Auth;

public interface IApiTokenStore
{
    string? GetAccessToken();
    void SetAccessToken(string accessToken);
    void Clear();
    Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct);
}