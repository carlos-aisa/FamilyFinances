namespace FamilyFinances.Web.Auth;

public interface ISessionInitializationService
{
    bool IsInitialized { get; }
    Task WaitForInitializationAsync();
    void MarkAsInitialized();
}

public sealed class SessionInitializationService : ISessionInitializationService
{
    private readonly TaskCompletionSource _initializationTcs = new();

    public bool IsInitialized { get; private set; }

    public Task WaitForInitializationAsync() => _initializationTcs.Task;

    public void MarkAsInitialized()
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        _initializationTcs.TrySetResult();
    }
}