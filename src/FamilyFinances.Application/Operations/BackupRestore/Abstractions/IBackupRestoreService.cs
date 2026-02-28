using FamilyFinances.Application.Operations.BackupRestore.Dtos;

namespace FamilyFinances.Application.Operations.BackupRestore.Abstractions;

public interface IBackupRestoreService
{
    Task<BackupArtifactDto> CreateBackupAsync(CancellationToken ct);

    Task<RestorePrecheckResultDto> PrecheckRestoreAsync(
        Stream packageStream,
        CancellationToken ct);

    Task<RestoreApplyResultDto> ApplyRestoreAsync(
        Stream packageStream,
        CancellationToken ct);
}
