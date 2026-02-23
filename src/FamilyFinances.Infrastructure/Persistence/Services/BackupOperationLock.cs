using FamilyFinances.Application.Operations.BackupRestore.Abstractions;

namespace FamilyFinances.Infrastructure.Persistence.Services;

public sealed class BackupOperationLock : IBackupOperationLock
{
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public async Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken ct)
    {
        if (!await _mutex.WaitAsync(0, ct))
            return null;

        return new Releaser(_mutex);
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private SemaphoreSlim? _mutex;

        public Releaser(SemaphoreSlim mutex)
        {
            _mutex = mutex;
        }

        public ValueTask DisposeAsync()
        {
            Interlocked.Exchange(ref _mutex, null)?.Release();
            return ValueTask.CompletedTask;
        }
    }
}
