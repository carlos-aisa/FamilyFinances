using FamilyFinances.Application.Operations.BackupRestore.Abstractions;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Application.Operations.BackupRestore.Exceptions;

namespace FamilyFinances.Application.Operations.BackupRestore.Handlers;

public sealed class PrecheckRestoreHandler
{
    private readonly IBackupRestoreService _service;
    private readonly IBackupOperationLock _operationLock;

    public PrecheckRestoreHandler(
        IBackupRestoreService service,
        IBackupOperationLock operationLock)
    {
        _service = service;
        _operationLock = operationLock;
    }

    public async Task<RestorePrecheckResultDto> HandleAsync(Stream packageStream, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(packageStream);

        await using var lockHandle = await _operationLock.TryAcquireAsync(ct);
        if (lockHandle is null)
            throw new BackupOperationInProgressException();

        await using var bufferedPackage = await CopyToSeekableStreamAsync(packageStream, ct);
        return await _service.PrecheckRestoreAsync(bufferedPackage, ct);
    }

    private static async Task<MemoryStream> CopyToSeekableStreamAsync(Stream source, CancellationToken ct)
    {
        var buffered = new MemoryStream();
        await source.CopyToAsync(buffered, ct);
        buffered.Position = 0;
        return buffered;
    }
}
