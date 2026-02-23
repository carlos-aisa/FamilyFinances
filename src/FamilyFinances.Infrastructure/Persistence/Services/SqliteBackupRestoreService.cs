using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using FamilyFinances.Application.Operations.BackupRestore.Abstractions;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyFinances.Infrastructure.Persistence.Services;

public sealed class SqliteBackupRestoreService : IBackupRestoreService
{
    private const string SupportedFormatVersion = "1.0";
    private const string BackupContentType = "application/octet-stream";
    private const string ManifestEntryName = "manifest.json";
    private const string DatabaseEntryName = "database.sqlite";
    private const string ExpectedPackageExtension = ".ffbackup";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly string[] RequiredTables =
    [
        "__EFMigrationsHistory",
        "AspNetUsers",
        "AspNetRoles",
        "AspNetUserRoles",
        "Accounts",
        "Payees",
        "Transactions",
        "TransactionSplits",
        "TransactionLinks",
        "AccountGroups",
        "AccountGroupMembers",
        "AccountYearSnapshots",
        "FiscalYearClosures"
    ];

    private readonly string _liveConnectionString;
    private readonly ILogger<SqliteBackupRestoreService> _logger;
    private readonly string? _currentAppVersion;

    public SqliteBackupRestoreService(
        IConfiguration configuration,
        ILogger<SqliteBackupRestoreService> logger)
    {
        _liveConnectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required for backup/restore.");
        _logger = logger;
        _currentAppVersion = ResolveCurrentAppVersion();
    }

    public async Task<BackupArtifactDto> CreateBackupAsync(CancellationToken ct)
    {
        var createdAtUtc = DateTimeOffset.UtcNow;
        var snapshotPath = CreateTempDatabasePath("ffbackup-export-snapshot");

        try
        {
            await BackupDatabaseAsync(
                _liveConnectionString,
                BuildSqliteFileConnectionString(snapshotPath),
                ct);

            var snapshotBytes = await ReadAllBytesWithSharedAccessAsync(snapshotPath, ct);
            var checksum = ComputeSha256(snapshotBytes);
            var sourceMigration = await ReadLatestMigrationAsync(snapshotPath, ct);
            var manifest = new BackupPackageManifestDto(
                SupportedFormatVersion,
                _currentAppVersion,
                createdAtUtc,
                sourceMigration,
                checksum,
                RequiredTables);

            var packageBytes = await BuildPackageAsync(manifest, snapshotBytes, ct);
            var fileName = BuildBackupFileName(createdAtUtc);

            return new BackupArtifactDto(fileName, BackupContentType, packageBytes, manifest);
        }
        finally
        {
            TryDeleteFile(snapshotPath);
        }
    }

    public async Task<RestorePrecheckResultDto> PrecheckRestoreAsync(
        Stream packageStream,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(packageStream);

        var parsedPackage = await ReadPackageAsync(packageStream, ct);
        var errors = parsedPackage.Errors.ToList();
        var warnings = parsedPackage.Warnings.ToList();

        if (parsedPackage.Manifest is null || parsedPackage.DatabasePayload is null)
        {
            return ToPrecheckResult(parsedPackage.Manifest, errors, warnings);
        }

        ValidateManifestFields(parsedPackage.Manifest, errors);

        if (errors.Count == 0)
        {
            await ValidateDatabasePayloadAsync(
                parsedPackage.Manifest,
                parsedPackage.DatabasePayload,
                errors,
                warnings,
                ct);
        }

        return ToPrecheckResult(parsedPackage.Manifest, errors, warnings);
    }

    public async Task<RestoreApplyResultDto> ApplyRestoreAsync(
        Stream packageStream,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(packageStream);

        await using var bufferedPackage = await CopyToSeekableStreamAsync(packageStream, ct);
        bufferedPackage.Position = 0;
        var precheck = await PrecheckRestoreAsync(bufferedPackage, ct);
        if (!precheck.IsCompatible)
        {
            return new RestoreApplyResultDto(
                Applied: false,
                AppliedAtUtc: null,
                RequiresReauthentication: false,
                FormatVersion: precheck.FormatVersion,
                SourceAppVersion: precheck.SourceAppVersion,
                SourceMigration: precheck.SourceMigration,
                Errors: precheck.Errors,
                Warnings: precheck.Warnings);
        }

        bufferedPackage.Position = 0;
        var parsedPackage = await ReadPackageAsync(bufferedPackage, ct);
        if (parsedPackage.Manifest is null || parsedPackage.DatabasePayload is null)
        {
            var errors = parsedPackage.Errors.Count == 0
                ? new[] { "Backup package could not be parsed during restore apply." }
                : parsedPackage.Errors.ToArray();

            return new RestoreApplyResultDto(
                Applied: false,
                AppliedAtUtc: null,
                RequiresReauthentication: false,
                FormatVersion: parsedPackage.Manifest?.FormatVersion,
                SourceAppVersion: parsedPackage.Manifest?.AppVersion,
                SourceMigration: parsedPackage.Manifest?.SourceMigration,
                Errors: errors,
                Warnings: parsedPackage.Warnings.ToArray());
        }

        var candidatePath = CreateTempDatabasePath("ffbackup-restore-candidate");
        var rollbackPath = CreateTempDatabasePath("ffbackup-restore-rollback");

        try
        {
            await File.WriteAllBytesAsync(candidatePath, parsedPackage.DatabasePayload, ct);
            await BackupDatabaseAsync(
                _liveConnectionString,
                BuildSqliteFileConnectionString(rollbackPath),
                ct);

            try
            {
                await BackupDatabaseAsync(
                    BuildSqliteFileConnectionString(candidatePath, SqliteOpenMode.ReadOnly),
                    _liveConnectionString,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Restore apply failed; attempting rollback.");

                var errors = new List<string>
                {
                    "Restore apply failed. The previous state was restored."
                };

                try
                {
                    await BackupDatabaseAsync(
                        BuildSqliteFileConnectionString(rollbackPath, SqliteOpenMode.ReadOnly),
                        _liveConnectionString,
                        ct);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback after restore failure also failed.");
                    errors.Add("Rollback attempt failed. Manual database recovery may be required.");
                }

                return new RestoreApplyResultDto(
                    Applied: false,
                    AppliedAtUtc: null,
                    RequiresReauthentication: false,
                    FormatVersion: parsedPackage.Manifest.FormatVersion,
                    SourceAppVersion: parsedPackage.Manifest.AppVersion,
                    SourceMigration: parsedPackage.Manifest.SourceMigration,
                    Errors: errors,
                    Warnings: precheck.Warnings);
            }

            return new RestoreApplyResultDto(
                Applied: true,
                AppliedAtUtc: DateTimeOffset.UtcNow,
                RequiresReauthentication: true,
                FormatVersion: parsedPackage.Manifest.FormatVersion,
                SourceAppVersion: parsedPackage.Manifest.AppVersion,
                SourceMigration: parsedPackage.Manifest.SourceMigration,
                Errors: Array.Empty<string>(),
                Warnings: precheck.Warnings);
        }
        finally
        {
            TryDeleteFile(candidatePath);
            TryDeleteFile(rollbackPath);
        }
    }

    private static async Task<byte[]> BuildPackageAsync(
        BackupPackageManifestDto manifest,
        byte[] databasePayload,
        CancellationToken ct)
    {
        await using var packageStream = new MemoryStream();
        using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifestEntry = archive.CreateEntry(ManifestEntryName, CompressionLevel.Optimal);
            await using (var manifestStream = manifestEntry.Open())
            {
                await JsonSerializer.SerializeAsync(manifestStream, manifest, JsonOptions, ct);
            }

            var databaseEntry = archive.CreateEntry(DatabaseEntryName, CompressionLevel.Optimal);
            await using (var databaseStream = databaseEntry.Open())
            {
                await databaseStream.WriteAsync(databasePayload, ct);
            }
        }

        return packageStream.ToArray();
    }

    private async Task ValidateDatabasePayloadAsync(
        ManifestPayload manifest,
        byte[] databasePayload,
        ICollection<string> errors,
        ICollection<string> warnings,
        CancellationToken ct)
    {
        var checksum = ComputeSha256(databasePayload);
        if (!string.Equals(
                NormalizeChecksum(manifest.DatabaseChecksumSha256),
                NormalizeChecksum(checksum),
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Backup payload checksum mismatch.");
            return;
        }

        var candidatePath = CreateTempDatabasePath("ffbackup-precheck-candidate");
        try
        {
            await File.WriteAllBytesAsync(candidatePath, databasePayload, ct);
            await using var candidateConnection = new SqliteConnection(
                BuildSqliteFileConnectionString(candidatePath, SqliteOpenMode.ReadOnly));
            await candidateConnection.OpenAsync(ct);

            await ValidateIntegrityChecksAsync(candidateConnection, errors, ct);
            await ValidateRequiredTablesAsync(candidateConnection, manifest, errors, ct);
            await ValidateMigrationBaselineAsync(candidateConnection, manifest, errors, warnings, ct);
            ValidateAppVersionCompatibility(manifest, warnings);
        }
        catch (SqliteException ex)
        {
            _logger.LogWarning(ex, "Backup package database validation failed.");
            errors.Add("Backup database structure validation failed.");
        }
        finally
        {
            TryDeleteFile(candidatePath);
        }
    }

    private static async Task ValidateIntegrityChecksAsync(
        SqliteConnection connection,
        ICollection<string> errors,
        CancellationToken ct)
    {
        await using var integrityCommand = connection.CreateCommand();
        integrityCommand.CommandText = "PRAGMA integrity_check;";
        var integrityResult = (await integrityCommand.ExecuteScalarAsync(ct))?.ToString();
        if (!string.Equals(integrityResult, "ok", StringComparison.OrdinalIgnoreCase))
            errors.Add("SQLite integrity check failed.");

        await using var foreignKeyCommand = connection.CreateCommand();
        foreignKeyCommand.CommandText = "PRAGMA foreign_key_check;";
        await using var reader = await foreignKeyCommand.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
            errors.Add("SQLite foreign-key consistency check failed.");
    }

    private static async Task ValidateRequiredTablesAsync(
        SqliteConnection connection,
        ManifestPayload manifest,
        ICollection<string> errors,
        CancellationToken ct)
    {
        var requiredTables = manifest.RequiredTables?
            .Where(table => !string.IsNullOrWhiteSpace(table))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? Array.Empty<string>();

        if (requiredTables.Length == 0)
        {
            errors.Add("Backup manifest is missing required table metadata.");
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";
        await using var reader = await command.ExecuteReaderAsync(ct);

        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(ct))
        {
            if (!reader.IsDBNull(0))
                existingTables.Add(reader.GetString(0));
        }

        var missingTables = requiredTables
            .Where(table => !existingTables.Contains(table))
            .OrderBy(table => table, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingTables.Length > 0)
            errors.Add($"Backup database is missing required tables: {string.Join(", ", missingTables)}.");
    }

    private async Task ValidateMigrationBaselineAsync(
        SqliteConnection candidateConnection,
        ManifestPayload manifest,
        ICollection<string> errors,
        ICollection<string> warnings,
        CancellationToken ct)
    {
        var candidateMigration = await ReadLatestMigrationAsync(candidateConnection, ct);
        if (string.IsNullOrWhiteSpace(candidateMigration))
        {
            errors.Add("Backup database does not contain migration baseline information.");
            return;
        }

        if (string.IsNullOrWhiteSpace(manifest.SourceMigration))
        {
            warnings.Add("Backup manifest does not include source migration metadata.");
        }
        else if (!string.Equals(manifest.SourceMigration, candidateMigration, StringComparison.Ordinal))
        {
            errors.Add("Backup manifest migration metadata does not match backup database migration baseline.");
            return;
        }

        await using var liveConnection = new SqliteConnection(_liveConnectionString);
        await liveConnection.OpenAsync(ct);

        var liveMigration = await ReadLatestMigrationAsync(liveConnection, ct);
        if (string.IsNullOrWhiteSpace(liveMigration))
        {
            warnings.Add("Current runtime database has no migration baseline metadata.");
            return;
        }

        if (!string.Equals(candidateMigration, liveMigration, StringComparison.Ordinal))
        {
            errors.Add(
                $"Migration baseline mismatch. Backup: '{candidateMigration}', current runtime: '{liveMigration}'.");
        }
    }

    private void ValidateAppVersionCompatibility(
        ManifestPayload manifest,
        ICollection<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(manifest.AppVersion))
        {
            warnings.Add("Backup manifest does not include source app version metadata.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentAppVersion))
            return;

        if (!TryParseVersionPrefix(manifest.AppVersion, out var sourceVersion))
            return;

        if (!TryParseVersionPrefix(_currentAppVersion, out var currentVersion))
            return;

        if (sourceVersion.Major != currentVersion.Major || sourceVersion.Minor != currentVersion.Minor)
        {
            warnings.Add(
                $"Backup app version '{manifest.AppVersion}' differs from current runtime version '{_currentAppVersion}'.");
        }
    }

    private static void ValidateManifestFields(ManifestPayload manifest, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(manifest.FormatVersion))
            errors.Add("Backup manifest is missing formatVersion.");

        if (manifest.CreatedAtUtc is null)
            errors.Add("Backup manifest is missing createdAtUtc.");

        if (string.IsNullOrWhiteSpace(manifest.DatabaseChecksumSha256))
            errors.Add("Backup manifest is missing databaseChecksumSha256.");

        if (manifest.RequiredTables is null || manifest.RequiredTables.Count == 0)
            errors.Add("Backup manifest is missing requiredTables.");

        if (!string.Equals(manifest.FormatVersion, SupportedFormatVersion, StringComparison.Ordinal))
        {
            errors.Add(
                $"Unsupported backup format version '{manifest.FormatVersion}'. Expected '{SupportedFormatVersion}'.");
        }
    }

    private static RestorePrecheckResultDto ToPrecheckResult(
        ManifestPayload? manifest,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        return new RestorePrecheckResultDto(
            IsCompatible: errors.Count == 0,
            FormatVersion: manifest?.FormatVersion,
            SourceAppVersion: manifest?.AppVersion,
            CreatedAtUtc: manifest?.CreatedAtUtc,
            SourceMigration: manifest?.SourceMigration,
            Errors: errors.ToArray(),
            Warnings: warnings.ToArray());
    }

    private static async Task<ParsedPackage> ReadPackageAsync(Stream packageStream, CancellationToken ct)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        await using var bufferedPackage = await CopyToSeekableStreamAsync(packageStream, ct);
        if (bufferedPackage.Length == 0)
        {
            errors.Add("Backup package is empty.");
            return new ParsedPackage(null, null, errors, warnings);
        }

        if (bufferedPackage.CanSeek)
            bufferedPackage.Position = 0;

        try
        {
            using var archive = new ZipArchive(bufferedPackage, ZipArchiveMode.Read, leaveOpen: true);

            var manifestEntry = archive.GetEntry(ManifestEntryName);
            if (manifestEntry is null)
            {
                errors.Add($"Backup package does not contain '{ManifestEntryName}'.");
                return new ParsedPackage(null, null, errors, warnings);
            }

            var databaseEntry = archive.GetEntry(DatabaseEntryName);
            if (databaseEntry is null)
            {
                errors.Add($"Backup package does not contain '{DatabaseEntryName}'.");
                return new ParsedPackage(null, null, errors, warnings);
            }

            ManifestPayload? manifest;
            await using (var manifestStream = manifestEntry.Open())
            {
                manifest = await JsonSerializer.DeserializeAsync<ManifestPayload>(manifestStream, JsonOptions, ct);
            }

            if (manifest is null)
            {
                errors.Add("Backup manifest could not be parsed.");
                return new ParsedPackage(null, null, errors, warnings);
            }

            byte[] databasePayload;
            await using (var databaseStream = databaseEntry.Open())
            await using (var databaseBuffer = new MemoryStream())
            {
                await databaseStream.CopyToAsync(databaseBuffer, ct);
                databasePayload = databaseBuffer.ToArray();
            }

            if (databasePayload.Length == 0)
                errors.Add("Backup database payload is empty.");

            return new ParsedPackage(manifest, databasePayload, errors, warnings);
        }
        catch (InvalidDataException)
        {
            errors.Add("Backup package is not a valid ZIP archive.");
            return new ParsedPackage(null, null, errors, warnings);
        }
    }

    private static async Task BackupDatabaseAsync(
        string sourceConnectionString,
        string destinationConnectionString,
        CancellationToken ct)
    {
        await using var source = new SqliteConnection(sourceConnectionString);
        await source.OpenAsync(ct);

        await using var destination = new SqliteConnection(destinationConnectionString);
        await destination.OpenAsync(ct);

        source.BackupDatabase(destination);
        destination.Close();
        source.Close();
    }

    private static async Task<MemoryStream> CopyToSeekableStreamAsync(
        Stream source,
        CancellationToken ct)
    {
        if (source.CanSeek)
            source.Position = 0;

        var copy = new MemoryStream();
        await source.CopyToAsync(copy, ct);
        copy.Position = 0;
        return copy;
    }

    private static async Task<string?> ReadLatestMigrationAsync(
        string databasePath,
        CancellationToken ct)
    {
        await using var connection = new SqliteConnection(
            BuildSqliteFileConnectionString(databasePath, SqliteOpenMode.ReadOnly));
        await connection.OpenAsync(ct);
        return await ReadLatestMigrationAsync(connection, ct);
    }

    private static async Task<string?> ReadLatestMigrationAsync(
        SqliteConnection connection,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MigrationId
            FROM __EFMigrationsHistory
            ORDER BY MigrationId DESC
            LIMIT 1;
            """;

        try
        {
            var value = await command.ExecuteScalarAsync(ct);
            return value as string;
        }
        catch (SqliteException)
        {
            return null;
        }
    }

    private static string BuildBackupFileName(DateTimeOffset createdAtUtc)
    {
        return $"familyfinances-backup-{createdAtUtc:yyyyMMdd-HHmmss}{ExpectedPackageExtension}";
    }

    private static async Task<byte[]> ReadAllBytesWithSharedAccessAsync(string path, CancellationToken ct)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    private static string BuildSqliteFileConnectionString(
        string filePath,
        SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = filePath,
            Mode = mode
        };

        return builder.ToString();
    }

    private static string CreateTempDatabasePath(string prefix)
    {
        return Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}.sqlite");
    }

    private static string ComputeSha256(byte[] payload)
    {
        var hash = SHA256.HashData(payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string NormalizeChecksum(string? checksum)
    {
        return string.IsNullOrWhiteSpace(checksum)
            ? string.Empty
            : checksum.Trim().ToLowerInvariant();
    }

    private static bool TryParseVersionPrefix(string rawVersion, out Version parsedVersion)
    {
        var normalized = rawVersion.Split(['-', '+'], 2, StringSplitOptions.TrimEntries)[0];
        return Version.TryParse(normalized, out parsedVersion!);
    }

    private static string? ResolveCurrentAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
            return informationalVersion.Split('+', 2, StringSplitOptions.TrimEntries)[0];

        return assembly.GetName().Version?.ToString();
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Temp file cleanup failed for {Path}", path);
        }
    }

    private sealed class ManifestPayload
    {
        public string? FormatVersion { get; set; }
        public string? AppVersion { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; }
        public string? SourceMigration { get; set; }
        public string? DatabaseChecksumSha256 { get; set; }
        public List<string>? RequiredTables { get; set; }
    }

    private sealed record ParsedPackage(
        ManifestPayload? Manifest,
        byte[]? DatabasePayload,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings);
}
