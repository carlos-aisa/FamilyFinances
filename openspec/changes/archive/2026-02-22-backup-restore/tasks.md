## 1. Application contracts and orchestration

- [x] 1.1 Add Backup/Restore DTO contracts in `src/FamilyFinances.Application/Operations/BackupRestore/Dtos/` (artifact metadata, pre-check result, apply result, deterministic error/warning fields).
- [x] 1.2 Add Application abstractions `IBackupRestoreService` and `IBackupOperationLock` in `src/FamilyFinances.Application/Operations/BackupRestore/Abstractions/`.
- [x] 1.3 Implement handlers `CreateBackupHandler`, `PrecheckRestoreHandler`, and `ApplyRestoreHandler` in `src/FamilyFinances.Application/Operations/BackupRestore/Handlers/` with cancellation-token propagation and validation-before-apply invariant.
- [x] 1.4 Add/update Application-level exceptions for incompatible package, busy operation, and restore failure with messages suitable for API mapping.

## 2. Infrastructure backup/restore implementation

- [x] 2.1 Implement `SqliteBackupRestoreService` in `src/FamilyFinances.Infrastructure/Persistence/Services/SqliteBackupRestoreService.cs` using SQLite backup primitives for export and apply.
- [x] 2.2 Implement backup package format `.ffbackup` (ZIP) including `manifest.json` + `database.sqlite` with format/app version and UTC timestamp.
- [x] 2.3 Add pre-check validation pipeline (archive structure, manifest compatibility, required tables, checksum/consistency checks) before restore apply.
- [x] 2.4 Add `BackupOperationLock` serialization service (single-operation maintenance lock) and ensure conflict signaling when concurrent operations are requested.
- [x] 2.5 Ensure temporary-file lifecycle is safe (`try/finally` cleanup) and uploaded files are never persisted permanently after operation completion.
- [x] 2.6 Register new services and handlers in `src/FamilyFinances.Infrastructure/DependencyInjection.cs`.

## 3. API endpoints and authorization

- [x] 3.1 Add `src/FamilyFinances.Api/Controllers/V1/BackupController.cs` with versioned routes: `GET /api/v1/backup/export`, `POST /api/v1/backup/restore/precheck`, `POST /api/v1/backup/restore/apply`.
- [x] 3.2 Enforce `[Authorize(Policy = Policies.CanWrite)]` on all backup/restore endpoints and keep unauthorized access behavior aligned with existing middleware/policies.
- [x] 3.3 Implement binary download response with deterministic filename `familyfinances-backup-YYYYMMDD-HHmmss.ffbackup` and proper content type.
- [x] 3.4 Implement multipart upload handling for pre-check/apply endpoints, including request-size limit and deterministic validation error responses.
- [x] 3.5 Return explicit conflict semantics (`409`) when maintenance lock is busy and include machine-readable error reason in payload.

## 4. Web Settings integration and user flows

- [x] 4.1 Add Settings route scaffolding in Web UI (`/settings` and `/settings/backup-restore`) under `src/FamilyFinances.Web/Components/Pages/Settings/`.
- [x] 4.2 Update `src/FamilyFinances.Web/Components/Layout/NavMenu.razor` to include authenticated Settings access in the navigation shell, with entry path to Backup/Restore via `/settings`.
- [x] 4.3 Add `src/FamilyFinances.Web/Api/BackupApi.cs` with methods for export download, pre-check upload, and apply upload using existing token flow patterns.
- [x] 4.4 Implement Backup/Restore page UI with: safety notice, export action, file picker, pre-check summary card, incompatible-state blocking, and operation result messages.
- [x] 4.5 Implement destructive-action confirmation guard (required `RESTORE` confirmation text) before enabling restore apply button.
- [x] 4.6 Handle `RequiresReauthentication` response path by forcing logout/session refresh and showing user guidance after successful restore.

## 5. Tests (required behavioral coverage)

- [x] 5.1 Add Application tests in `tests/FamilyFinances.Application.Tests/` for handler orchestration, validation-before-apply, and lock contention behavior.
- [x] 5.2 Add Infrastructure-focused tests for package parsing/validation edge cases (malformed ZIP, incompatible manifest, missing payload entries).
- [x] 5.3 Add API integration tests in `tests/FamilyFinances.Api.IntegrationTests/` for: admin export success, non-admin forbidden/unauthorized, pre-check invalid package, apply success, and apply failure state-preservation.
- [x] 5.4 Add Web component tests in `tests/FamilyFinances.Web.Tests/Features/Settings/` for backup page rendering, confirmation guard, operation state transitions, and error/success messages.
- [x] 5.5 Ensure no EF InMemory usage is introduced for integration behavior; use relational provider-backed tests only.

## 6. Documentation and final validation

- [x] 6.1 Add/update backup-restore operational docs in `docs/` (how to export, how to restore, limitations, security handling notes, rollback guidance).
- [x] 6.2 Update `README.md` report/settings sections with backup/restore entry points and admin-only restrictions.
- [x] 6.3 Run Release validation suites for impacted layers and record outcomes in change notes:
- [x] 6.4 `dotnet test tests/FamilyFinances.Application.Tests/FamilyFinances.Application.Tests.csproj --configuration Release --filter "FullyQualifiedName~Backup|FullyQualifiedName~Restore"`
- [x] 6.5 `dotnet test tests/FamilyFinances.Api.IntegrationTests/FamilyFinances.Api.IntegrationTests.csproj --configuration Release --filter "FullyQualifiedName~Backup|FullyQualifiedName~Restore"`
- [x] 6.6 `dotnet test tests/FamilyFinances.Web.Tests/FamilyFinances.Web.Tests.csproj --configuration Release --filter "FullyQualifiedName~Settings|FullyQualifiedName~Backup|FullyQualifiedName~Restore"`
