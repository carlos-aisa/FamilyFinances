using FamilyFinances.Application.Operations.BackupRestore.Abstractions;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Application.Operations.BackupRestore.Exceptions;

namespace FamilyFinances.Application.Operations.BackupRestore.Handlers;

public sealed class CreateBackupHandler
{
    private readonly IBackupRestoreService _service;
    private readonly IBackupOperationLock _operationLock;

    public CreateBackupHandler(
        IBackupRestoreService service,
        IBackupOperationLock operationLock)
    {
        _service = service;
        _operationLock = operationLock;
    }

    public async Task<BackupArtifactDto> HandleAsync(CancellationToken ct)
    {
        await using var lockHandle = await _operationLock.TryAcquireAsync(ct);
        if (lockHandle is null)
            throw new BackupOperationInProgressException();

        return await _service.CreateBackupAsync(ct);
    }
}
