namespace FamilyFinances.Application.Operations.BackupRestore.Dtos;

public sealed record BackupPackageManifestDto(
    string FormatVersion,
    string? AppVersion,
    DateTimeOffset CreatedAtUtc,
    string? SourceMigration,
    string DatabaseChecksumSha256,
    IReadOnlyList<string> RequiredTables);

public sealed record BackupArtifactDto(
    string FileName,
    string ContentType,
    byte[] Content,
    BackupPackageManifestDto Manifest);

public sealed record RestorePrecheckResultDto(
    bool IsCompatible,
    string? FormatVersion,
    string? SourceAppVersion,
    DateTimeOffset? CreatedAtUtc,
    string? SourceMigration,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record RestoreApplyResultDto(
    bool Applied,
    DateTimeOffset? AppliedAtUtc,
    bool RequiresReauthentication,
    string? FormatVersion,
    string? SourceAppVersion,
    string? SourceMigration,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);
