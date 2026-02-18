namespace FamilyFinances.Web.State;

public sealed class HistoryRefreshNotifier
{
    private readonly object _gate = new();
    private readonly List<Action> _subscribers = new();

    public IDisposable Subscribe(Action callback)
    {
        lock (_gate)
        {
            _subscribers.Add(callback);
        }

        return new Subscription(this, callback);
    }

    public void NotifyChanged()
    {
        Action[] callbacks;

        lock (_gate)
        {
            callbacks = _subscribers.ToArray();
        }

        foreach (var callback in callbacks)
        {
            try
            {
                callback();
            }
            catch
            {
                // Keep notifying other subscribers even if one fails.
            }
        }
    }

    private void Unsubscribe(Action callback)
    {
        lock (_gate)
        {
            _subscribers.Remove(callback);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly HistoryRefreshNotifier _owner;
        private readonly Action _callback;
        private bool _disposed;

        public Subscription(HistoryRefreshNotifier owner, Action callback)
        {
            _owner = owner;
            _callback = callback;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner.Unsubscribe(_callback);
        }
    }
}
