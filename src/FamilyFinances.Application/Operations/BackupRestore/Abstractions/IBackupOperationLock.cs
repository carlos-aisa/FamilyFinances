namespace FamilyFinances.Application.Operations.BackupRestore.Abstractions;

public interface IBackupOperationLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken ct);
}
