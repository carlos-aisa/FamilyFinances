## Context

FamilyFinances currently has no first-class backup/restore workflow. Users depend on manual file-level handling, which is error-prone and undocumented. The Web app is Blazor Interactive Server and consumes the API via bearer token. The API uses two EF Core DbContexts (`AppIdentityDbContext`, `LedgerDbContext`) configured against the same SQLite connection string.

Current behavior and constraints relevant to this design:
- No Settings page exists yet; appearance controls currently live in `src/FamilyFinances.Web/Components/Layout/NavMenu.razor`.
- API authorization policies are role-based (`CanRead`, `CanWrite`) and `CanWrite` is `Admin` only.
- Runtime initialization migrates databases at startup (`DependencyInjection.InitializeAsync`).
- Release packaging is local-first (Windows distribution ZIP), so backup/restore must work offline and without external services.

Stakeholders:
- Admin users who need reliable export/import of full application state.
- Operators moving data between environments or recovering after user mistakes.
- Maintainers who need deterministic restore semantics and regression-safe behavior.

## Goals / Non-Goals

**Goals:**
- Add a full backup export flow from the Web app (Settings area) backed by API endpoints.
- Add a full restore flow from the Web app with strict validation before apply.
- Restore must be all-or-nothing from application perspective (no partial writes visible after failure).
- Ensure backup package includes all required data for a working instance, including Identity and ledger data.
- Add deterministic compatibility checks (format version, DB structure, migration baseline) before restore apply.
- Keep backup/restore operations explicit and user-confirmed with clear progress and outcome states.

**Non-Goals:**
- Scheduled background backups.
- Cloud storage integration.
- Partial object-level restore/merge.
- Cross-provider DB transformation (SQLite only in this iteration).
- End-user key management and backup encryption in this iteration.

## IMPLEMENTATION RULES - DO NOT DEVIATE

- [MUST] Keep architecture layering strict: Presentation -> Application -> Domain, Infrastructure -> Application/Domain.
- [MUST] Implement backup/restore as additive behavior; existing ledger/reporting features must remain unaffected when not invoked.
- [MUST] Restrict backup and restore execution to `CanWrite` (`Admin`) policy.
- [MUST] Use a dedicated maintenance lock to serialize backup/restore operations.
- [MUST] Validate backup package before any write operation to live database.
- [MUST] Keep restore atomic from observable behavior (success applies fully, failure leaves prior state intact).
- [MUST] Expose explicit success/failure payloads with actionable messages in UI.
- [MUST NOT] bypass EF/Application boundaries by implementing restore logic directly in controller classes.
- [MUST NOT] store uploaded backup packages permanently on server disk.
- [MUST NOT] introduce provider-specific behavior beyond SQLite for this change.

## DETAILED UI FLOWS

### Flow 1: Open Backup/Restore settings
1. Authenticated admin opens the navigation shell and clicks the Settings (gear) icon.
2. Admin lands on `/settings` and opens `Backup & Restore`.
3. Page loads capability summary, safety notice, and two action cards: `Create Backup` and `Restore from File`.
4. Non-admin users never see actionable controls (read-only guidance or access denied state).

### Flow 2: Create backup package
1. User clicks `Create Backup`.
2. UI disables button and calls `BackupApi.CreateBackupAsync()`.
3. API validates user policy and acquires maintenance lock.
4. API generates package stream and responds with downloadable file (`application/octet-stream`).
5. Browser download starts with deterministic file name format: `familyfinances-backup-YYYYMMDD-HHmmss.ffbackup`.
6. UI shows non-blocking success message with UTC timestamp.

### Flow 3: Select restore file and run pre-check
1. User clicks `Select backup file` and chooses `.ffbackup`.
2. UI performs basic client-side checks (extension, non-zero size) and uploads file for server validation.
3. API parses archive, reads manifest, validates schema/version/table baseline.
4. API returns `RestorePrecheckResult` with `IsCompatible`, warnings, and detected snapshot metadata.
5. UI renders a pre-check summary card and enables `Apply Restore` only when compatible.

### Flow 4: Confirm and apply restore
1. User clicks `Apply Restore`.
2. UI requires explicit confirmation text (for example: type `RESTORE`) before submitting.
3. API re-validates package and acquires maintenance lock.
4. API applies restore using SQLite backup mechanism to live DB connection in single controlled operation.
5. API returns result payload with `AppliedAtUtc`, snapshot metadata, and required post-actions (re-login).
6. UI shows success banner and prompts user to re-authenticate.

### Flow 5: Restore failure path
1. Any validation failure returns deterministic error payload without touching live DB.
2. Any apply failure returns failure payload and preserves previous DB state.
3. UI shows detailed error message and keeps `Create Backup` available.

### Flow 6: Concurrent operation handling
1. If backup/restore already in progress, API returns `409 Conflict` with reason `OperationInProgress`.
2. UI shows "Another maintenance operation is running" and no retry loop by default.

## DETAILED PAGE WIREFRAMES

### Navigation shell update

```text
+--------------------------------------------------+
| FamilyFinances                                   |
| [settings icon]                                  |
+--------------------------------------------------+
| Home                                             |
| Accounts                                         |
| Account Groups                                   |
| Payees                                           |
| Transactions                                     |
| History                                          |
| Reports                                          |
| Logout                                           |
+--------------------------------------------------+
```

### Backup/Restore page (`/settings/backup-restore`)

```text
+----------------------------------------------------------------------------------+
| Backup & Restore                                                                 |
| Export current data or restore from a validated backup package.                  |
+----------------------------------------------------------------------------------+
| Safety Notice                                                                     |
| - Restore replaces current data completely                                       |
| - Admin only                                                                      |
| - Create fresh backup before restore                                             |
+----------------------------------------------------------------------------------+
| Create Backup                                | Restore from File                 |
| [Create Backup]                              | [Choose File] [.ffbackup]        |
| Last export: 2026-02-22 10:15 UTC           | Pre-check: Compatible/Warnings    |
|                                              | [Apply Restore] (confirm required)|
+----------------------------------------------------------------------------------+
| Operation Log (session-local)                                                       |
| - Backup created                                                                   |
| - Restore pre-check failed: ...                                                    |
| - Restore applied at ...                                                           |
+----------------------------------------------------------------------------------+
```

### Restore confirmation modal

```text
+------------------------------------------------------+
| Confirm Restore                                       |
| This will replace all current data.                  |
| Type RESTORE to continue: [__________]              |
| [Cancel]                               [Apply]       |
+------------------------------------------------------+
```

## COMPONENT REUSE MATRIX

| Area | Reuse | Modify | New |
|---|---|---|---|
| Navigation shell | `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` | Add authenticated Settings icon entry point to `/settings` | None |
| Existing auth/session model | `JwtAuthStateProvider`, `IApiTokenStore` | Reuse for backup API client calls | None |
| API versioned routing style | `src/FamilyFinances.Api/Controllers/V1/*.cs` | Add a new controller under `V1` with same policy pattern | `src/FamilyFinances.Api/Controllers/V1/BackupController.cs` |
| Web API client pattern | `src/FamilyFinances.Web/Api/ReportsApi.cs` style | Add binary upload/download support | `src/FamilyFinances.Web/Api/BackupApi.cs` |
| Application handler style | Existing CQRS handlers in `Application/*/Handlers` | Add backup/restore handlers and DTOs | `src/FamilyFinances.Application/Operations/BackupRestore/*` |
| Persistence services | Existing infra service registration and DbContext wiring | Add SQLite backup/restore service | `src/FamilyFinances.Infrastructure/Persistence/Services/SqliteBackupRestoreService.cs` |
| Settings page patterns | Existing page composition style (`card`, `form`, alerts) | Add Settings subtree | `src/FamilyFinances.Web/Components/Pages/Settings/BackupRestorePage.razor` |
| Domain exception mapping | `DomainExceptionMiddleware` | Reuse for validation/operation error mapping | Optional dedicated exception types in Application layer |

## Decisions

### Decision 1: Backup package format is ZIP + manifest + SQLite snapshot
- **Choice:** `.ffbackup` ZIP with:
  - `manifest.json` (formatVersion, appVersion, createdAtUtc, sourceMigration, checksum, notes)
  - `database.sqlite` (full snapshot)
- **Rationale:** portable, inspectable, and future-extensible without breaking current contract.
- **Alternative considered:** raw `.db` file only.
  - **Rejected because:** no metadata/versioning channel and weak compatibility diagnostics.

### Decision 2: Use SQLite backup primitives for backup and restore
- **Choice:** Use `Microsoft.Data.Sqlite` backup operation between source and destination connections.
- **Rationale:** consistent SQLite-native copy semantics, lower risk than ad-hoc SQL copy scripts.
- **Alternative considered:** table-by-table SQL import/export.
  - **Rejected because:** higher complexity and higher risk for constraints/index/state drift.

### Decision 3: Maintenance lock serializes backup/restore
- **Choice:** process-wide lock (for example `SemaphoreSlim(1,1)`) wrapped by an Application service.
- **Rationale:** prevents concurrent operations and reduces lock/contention issues.
- **Alternative considered:** no explicit lock, rely on DB file locking only.
  - **Rejected because:** poor UX and nondeterministic conflict behavior.

### Decision 4: Restore is replace-style, not merge-style
- **Choice:** restore overwrites full DB state from backup snapshot.
- **Rationale:** deterministic and simpler to reason about/validate.
- **Alternative considered:** partial/merge restore.
  - **Rejected because:** conflict semantics become complex and dangerous in `0.9.x`.

### Decision 5: Authorization uses existing `CanWrite` policy
- **Choice:** gate all backup/restore endpoints with `CanWrite` (Admin).
- **Rationale:** aligns with current role model and avoids introducing parallel authorization logic.
- **Alternative considered:** custom `CanOperateMaintenance` policy.
  - **Rejected because:** unnecessary policy expansion for this iteration.

### Decision 6: No at-rest backup encryption in first iteration
- **Choice:** keep plain package format and document handling responsibilities.
- **Rationale:** keeps MVP operationally feasible in `0.9.x`.
- **Alternative considered:** passphrase-based encrypted archive.
  - **Rejected because:** key management UX/security scope exceeds current release objective.

### Decision 7: Pre-check endpoint before restore apply
- **Choice:** split restore flow into:
  - pre-check (`validate only`)
  - apply (`validate + restore`)
- **Rationale:** safer UX and clearer error surface.
- **Alternative considered:** single-step upload+apply.
  - **Rejected because:** less transparent and riskier for user operations.

## Risks / Trade-offs

- [Risk] Backup package contains sensitive data (identity hashes, ledger history) -> Mitigation: Admin-only endpoints, no server persistence of uploaded files, explicit handling guidance in docs.
- [Risk] Large backup files can increase memory/disk pressure -> Mitigation: file size limit, stream-based processing, temp-file cleanup in `finally`.
- [Risk] Restore can invalidate active sessions if identity data changes -> Mitigation: return `RequiresReauthentication=true` and force client logout on successful restore.
- [Risk] Schema drift between backup and running app can cause restore failures -> Mitigation: manifest + migration pre-check with clear compatibility errors.
- [Risk] Concurrent write operations may cause lock contention -> Mitigation: maintenance lock + short operation window + deterministic conflict responses.
- [Risk] Corrupted backup file could trigger runtime exceptions -> Mitigation: strict archive validation and defensive exception mapping.
- [Risk] Replace restore can lose recent unsaved/unknown state -> Mitigation: mandatory warning, confirmation gate, and recommendation to create fresh backup before apply.
- [Risk] SQLite-specific design limits future DB portability -> Mitigation: isolate implementation behind `IBackupRestoreService` abstraction.

## Migration Plan

1. Add Application contracts and handlers:
   - `CreateBackupHandler`
   - `PrecheckRestoreHandler`
   - `ApplyRestoreHandler`
   - DTOs for manifest/pre-check/result.
2. Add Infrastructure service implementation (`SqliteBackupRestoreService`) with:
   - package assembly/disassembly
   - validation pipeline
   - restore execution and temp-file lifecycle.
3. Register service and handlers in `src/FamilyFinances.Infrastructure/DependencyInjection.cs`.
4. Add API controller (`/api/v1/backup/*`) with `CanWrite` policy and consistent error mapping.
5. Add Web API client (`BackupApi`) with binary download/upload support.
6. Add Web UI route/page under Settings and integrate navigation shell access to `/settings`.
7. Add tests:
   - API integration tests (auth, valid backup, invalid package, conflict)
   - Application tests for pre-check and restore orchestration
   - Web component tests for flow state and confirmation behavior.
8. Update docs:
   - operational guide
   - known limitations
   - recovery checklist.
9. Rollback strategy:
   - hide/disable UI entry
   - disable controller endpoints
   - keep existing DB untouched (no migration dependency for rollback).

## CODE EXAMPLES FOR CRITICAL COMPONENTS

### Example 1: API endpoint signatures (`BackupController`)

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/backup")]
[Authorize(Policy = Policies.CanWrite)]
public sealed class BackupController : ControllerBase
{
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct) { ... }

    [HttpPost("restore/precheck")]
    [RequestSizeLimit(209_715_200)] // 200 MB
    public async Task<IActionResult> Precheck(IFormFile file, CancellationToken ct) { ... }

    [HttpPost("restore/apply")]
    [RequestSizeLimit(209_715_200)]
    public async Task<IActionResult> Apply(IFormFile file, CancellationToken ct) { ... }
}
```

### Example 2: Application abstraction

```csharp
public interface IBackupRestoreService
{
    Task<BackupArtifact> CreateBackupAsync(CancellationToken ct);
    Task<RestorePrecheckResult> PrecheckRestoreAsync(Stream packageStream, CancellationToken ct);
    Task<RestoreApplyResult> ApplyRestoreAsync(Stream packageStream, CancellationToken ct);
}
```

### Example 3: Infrastructure registration

```csharp
services.AddSingleton<IBackupOperationLock, BackupOperationLock>();
services.AddScoped<IBackupRestoreService, SqliteBackupRestoreService>();
services.AddScoped<CreateBackupHandler>();
services.AddScoped<PrecheckRestoreHandler>();
services.AddScoped<ApplyRestoreHandler>();
```

### Example 4: Web API client methods

```csharp
public sealed class BackupApi
{
    public Task<DownloadedFileDto> ExportBackupAsync(CancellationToken ct = default);
    public Task<RestorePrecheckDto> PrecheckRestoreAsync(IBrowserFile file, CancellationToken ct = default);
    public Task<RestoreApplyDto> ApplyRestoreAsync(IBrowserFile file, CancellationToken ct = default);
}
```

### Example 5: Restore confirm guard in Razor page

```razor
<InputText @bind-Value="_confirmationText" class="form-control" />
<button class="btn btn-danger"
        disabled="@(!string.Equals(_confirmationText, "RESTORE", StringComparison.Ordinal))"
        @onclick="ApplyRestoreAsync">
    Apply Restore
</button>
```

### Example 6: Deterministic pre-check response model

```csharp
public sealed record RestorePrecheckDto(
    bool IsCompatible,
    string FormatVersion,
    string? SourceAppVersion,
    DateTimeOffset? CreatedAtUtc,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);
```

## IMPLEMENTATION VERIFICATION CHECKLIST

### API and Contract Checks
- [ ] Backup export endpoint exists under versioned API route.
- [ ] Restore pre-check endpoint exists under versioned API route.
- [ ] Restore apply endpoint exists under versioned API route.
- [ ] All backup endpoints enforce `CanWrite`.
- [ ] Unauthorized calls return `401` or `403`.
- [ ] Conflict-in-progress path returns `409`.
- [ ] Validation failures return deterministic error payloads.
- [ ] Export endpoint returns binary content type.
- [ ] Export endpoint returns deterministic filename.
- [ ] Restore endpoints enforce max request size.
- [ ] Pre-check response contains compatibility boolean.
- [ ] Apply response contains applied timestamp.

### Application Layer Checks
- [ ] Handler exists for backup export orchestration.
- [ ] Handler exists for restore pre-check orchestration.
- [ ] Handler exists for restore apply orchestration.
- [ ] DTOs model errors and warnings explicitly.
- [ ] Restore apply path re-runs validation before write.
- [ ] Operation lock abstraction exists in Application boundary.
- [ ] No controller directly manipulates DB internals.
- [ ] Cancellation tokens are propagated end-to-end.
- [ ] Handler logging includes operation correlation id.
- [ ] Failure paths map to domain/application exceptions cleanly.
- [ ] Service abstraction allows future provider swap.
- [ ] No new circular dependencies introduced.

### Infrastructure Checks
- [ ] Backup package builder writes manifest and sqlite payload.
- [ ] Manifest includes format version.
- [ ] Manifest includes app version.
- [ ] Manifest includes created timestamp (UTC).
- [ ] Temp files are always deleted in finally blocks.
- [ ] SQLite backup primitive is used for snapshot.
- [ ] SQLite backup primitive is used for restore apply.
- [ ] Restore can operate without permanent server file persistence.
- [ ] Concurrency lock prevents two simultaneous backup/restore jobs.
- [ ] Infrastructure service validates required tables.
- [ ] Infrastructure service rejects malformed archives.
- [ ] File size and stream handling are bounded and deterministic.

### Web UI Checks
- [ ] Navigation includes Settings entry for authenticated users.
- [ ] Backup/Restore page route is reachable.
- [ ] Backup button disabled while operation is running.
- [ ] Restore apply requires explicit confirmation text.
- [ ] Pre-check result is shown before apply action.
- [ ] Incompatible package blocks apply action.
- [ ] Success state clearly communicates next steps.
- [ ] Failure state clearly communicates reason.
- [ ] UI supports keyboard interaction for all controls.
- [ ] Accessible labels exist for file picker and action buttons.

### Security and Reliability Checks
- [ ] Backup endpoints are not exposed to readers.
- [ ] Uploaded package is never executed as code.
- [ ] Uploaded package is never stored permanently by default.
- [ ] Restore operation does not proceed after failed pre-check.
- [ ] Restore failure preserves previous DB state.
- [ ] Restore success returns `RequiresReauthentication` when needed.
- [ ] Error messages avoid leaking sensitive internals.
- [ ] Log entries include enough context for audits.
- [ ] Backup format version mismatch is handled explicitly.
- [ ] Busy operation conflicts are visible in UI.

### Tests and Documentation Checks
- [ ] API integration test: export success (admin).
- [ ] API integration test: export forbidden (reader).
- [ ] API integration test: pre-check invalid archive.
- [ ] API integration test: pre-check incompatible manifest.
- [ ] API integration test: apply restore success path.
- [ ] API integration test: apply restore failure preserves state.
- [ ] Application test: lock contention behavior.
- [ ] Application test: validation-before-apply invariant.
- [ ] Web component test: confirmation guard.
- [ ] Web component test: operation state transitions.
- [ ] Docs include backup format and operational limits.
- [ ] Docs include restore warnings and rollback behavior.

## Open Questions

- Should backup package encryption (password-protected export) be included in `0.9.x` or deferred to `1.0`?
- Should restore force immediate logout server-side, or should UI perform a coordinated post-restore logout flow?
- What is the final accepted max upload size for `.ffbackup` in default distribution builds?
