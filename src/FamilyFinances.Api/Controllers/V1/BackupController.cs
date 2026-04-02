using Asp.Versioning;
using FamilyFinances.Application.Operations.BackupRestore.Dtos;
using FamilyFinances.Application.Operations.BackupRestore.Exceptions;
using FamilyFinances.Application.Operations.BackupRestore.Handlers;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FamilyFinances.Infrastructure.Identity.AuthConstants;

namespace FamilyFinances.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/backup")]
[Authorize(Policy = Policies.CanWrite)]
public sealed class BackupController : ControllerBase
{
    private const long MaxUploadSizeBytes = 209_715_200; // 200 MB

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromServices] CreateBackupHandler handler,
        CancellationToken ct)
    {
        try
        {
            var artifact = await handler.HandleAsync(ct);
            return File(artifact.Content, artifact.ContentType, artifact.FileName);
        }
        catch (BackupOperationInProgressException ex)
        {
            return Conflict(new { error = ex.Message, reason = "OperationInProgress" });
        }
    }

    [HttpGet("database-info")]
    public ActionResult<BackupDatabaseInfoDto> GetDatabaseInfo(
        [FromServices] IConfiguration configuration,
        [FromServices] IWebHostEnvironment hostEnvironment)
    {
        var connectionString = configuration.GetConnectionString("Default");
        var databaseFilePath = ResolveDatabaseFilePath(connectionString, hostEnvironment.ContentRootPath);
        return Ok(new BackupDatabaseInfoDto(databaseFilePath));
    }

    [HttpPost("restore/precheck")]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<ActionResult<RestorePrecheckResultDto>> Precheck(
        [FromForm] IFormFile? file,
        [FromServices] PrecheckRestoreHandler handler,
        CancellationToken ct)
    {
        var validationError = ValidateUpload(file);
        if (validationError is not null)
            return validationError;

        try
        {
            await using var packageStream = file!.OpenReadStream();
            var result = await handler.HandleAsync(packageStream, ct);
            return Ok(result);
        }
        catch (BackupOperationInProgressException ex)
        {
            return Conflict(new { error = ex.Message, reason = "OperationInProgress" });
        }
    }

    [HttpPost("restore/apply")]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<ActionResult<RestoreApplyResultDto>> Apply(
        [FromForm] IFormFile? file,
        [FromServices] ApplyRestoreHandler handler,
        CancellationToken ct)
    {
        var validationError = ValidateUpload(file);
        if (validationError is not null)
            return validationError;

        try
        {
            await using var packageStream = file!.OpenReadStream();
            var result = await handler.HandleAsync(packageStream, ct);
            return Ok(result);
        }
        catch (IncompatibleBackupPackageException ex)
        {
            return BadRequest(new { error = ex.Message, reason = "IncompatiblePackage" });
        }
        catch (BackupRestoreApplyException ex)
        {
            return BadRequest(new { error = ex.Message, reason = "RestoreApplyFailed" });
        }
        catch (BackupOperationInProgressException ex)
        {
            return Conflict(new { error = ex.Message, reason = "OperationInProgress" });
        }
    }

    private static BadRequestObjectResult? ValidateUpload(IFormFile? file)
    {
        if (file is null)
            return new BadRequestObjectResult(new { error = "Form field 'file' is required.", reason = "MissingFile" });

        if (file.Length <= 0)
            return new BadRequestObjectResult(new { error = "Backup file is empty.", reason = "EmptyFile" });

        if (!file.FileName.EndsWith(".ffbackup", StringComparison.OrdinalIgnoreCase))
        {
            return new BadRequestObjectResult(
                new { error = "Backup file must use the .ffbackup extension.", reason = "InvalidFileType" });
        }

        if (file.Length > MaxUploadSizeBytes)
        {
            return new BadRequestObjectResult(
                new { error = "Backup file exceeds the maximum allowed size (200 MB).", reason = "FileTooLarge" });
        }

        return null;
    }

    private static string? ResolveDatabaseFilePath(string? connectionString, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.DataSource))
                return null;

            var dataSource = builder.DataSource.Trim();
            if (string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
                return dataSource;

            if (Uri.TryCreate(dataSource, UriKind.Absolute, out var uri) && uri.IsFile)
                return Path.GetFullPath(uri.LocalPath);

            if (Path.IsPathRooted(dataSource))
                return Path.GetFullPath(dataSource);

            return Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}

public sealed record BackupDatabaseInfoDto(string? DatabaseFilePath);
