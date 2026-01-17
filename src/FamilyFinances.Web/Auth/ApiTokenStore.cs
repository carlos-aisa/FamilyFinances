namespace FamilyFinances.Web.Auth;

public sealed class ApiTokenStore : IApiTokenStore
{
    private string? _token;
    private readonly TaskCompletionSource<string?> _tokenReady =
       new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string? GetAccessToken() => _token;

    public void SetAccessToken(string accessToken)
    {
        _token = accessToken;
        _tokenReady.TrySetResult(_token);
    }

    public void Clear()
    {
        _token = null;
    }

    public async Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_token))
            return _token;

        var completed = await Task.WhenAny(_tokenReady.Task, Task.Delay(timeout, ct));
        if (completed != _tokenReady.Task)
            return null;

        return await _tokenReady.Task;
    }
}
