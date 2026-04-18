namespace FamilyFinances.Web.Auth;

public sealed class ApiTokenStore : IApiTokenStore
{
    private readonly object _gate = new();
    private string? _token;
    private TaskCompletionSource<string?> _tokenReady =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string? GetAccessToken()
    {
        lock (_gate)
        {
            return _token;
        }
    }

    public void SetAccessToken(string accessToken)
    {
        lock (_gate)
        {
            _token = accessToken;
            _tokenReady.TrySetResult(_token);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _token = null;
            _tokenReady = new TaskCompletionSource<string?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public async Task<string?> WaitForTokenAsync(TimeSpan timeout, CancellationToken ct)
    {
        Task<string?> waitTask;
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(_token))
                return _token;

            waitTask = _tokenReady.Task;
        }

        var completed = await Task.WhenAny(waitTask, Task.Delay(timeout, ct));
        if (completed != waitTask)
            return null;

        return await waitTask;
    }
}
