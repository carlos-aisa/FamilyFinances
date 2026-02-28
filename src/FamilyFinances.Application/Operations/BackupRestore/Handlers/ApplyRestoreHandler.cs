using FamilyFinances.Application.Operations.BackupRestore.Abstractions;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Application.Operations.BackupRestore.Exceptions;

namespace FamilyFinances.Application.Operations.BackupRestore.Handlers;

public sealed class ApplyRestoreHandler
{
    private readonly IBackupRestoreService _service;
    private readonly IBackupOperationLock _operationLock;

    public ApplyRestoreHandler(
        IBackupRestoreService service,
        IBackupOperationLock operationLock)
    {
        _service = service;
        _operationLock = operationLock;
    }

    public async Task<RestoreApplyResultDto> HandleAsync(Stream packageStream, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(packageStream);

        await using var lockHandle = await _operationLock.TryAcquireAsync(ct);
        if (lockHandle is null)
            throw new BackupOperationInProgressException();

        await using var bufferedPackage = await CopyToSeekableStreamAsync(packageStream, ct);
        var precheck = await _service.PrecheckRestoreAsync(bufferedPackage, ct);
        if (!precheck.IsCompatible)
        {
            var message = precheck.Errors.Count == 0
                ? "Backup package is incompatible and cannot be restored."
                : string.Join(" ", precheck.Errors);
            throw new IncompatibleBackupPackageException(message);
        }

        bufferedPackage.Position = 0;
        var result = await _service.ApplyRestoreAsync(bufferedPackage, ct);
        if (!result.Applied && result.Errors.Count == 0)
            throw new BackupRestoreApplyException("Restore operation failed.");

        return result;
    }

    private static async Task<MemoryStream> CopyToSeekableStreamAsync(Stream source, CancellationToken ct)
    {
        var buffered = new MemoryStream();
        await source.CopyToAsync(buffered, ct);
        buffered.Position = 0;
        return buffered;
    }
}
