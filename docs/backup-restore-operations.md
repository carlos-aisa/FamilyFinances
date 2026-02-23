# Backup & Restore Operations

## Scope
`Backup & Restore` is an admin-only maintenance feature that exports or restores the full runtime SQLite state used by FamilyFinances (Identity + Ledger data).

The feature is available in the Web app at:
- `/settings/backup-restore`

API endpoints:
- `GET /api/v1/backup/export`
- `POST /api/v1/backup/restore/precheck`
- `POST /api/v1/backup/restore/apply`

## Backup Package Format
Export creates a `.ffbackup` ZIP package with:
- `manifest.json`
- `database.sqlite`

Manifest includes:
- `formatVersion`
- `appVersion`
- `createdAtUtc`
- `sourceMigration`
- `databaseChecksumSha256`
- `requiredTables`

The exported filename is deterministic:
- `familyfinances-backup-YYYYMMDD-HHmmss.ffbackup`

## Export Procedure
1. Open `/settings/backup-restore` as an admin.
2. Click `Create Backup`.
3. Browser download starts automatically.
4. Store the file in a secure location.

## Restore Procedure
1. Open `/settings/backup-restore` as an admin.
2. Select a `.ffbackup` file.
3. Wait for `Pre-check Result`.
4. Confirm by typing `RESTORE`.
5. Click `Apply Restore`.

Restore is blocked unless pre-check is compatible and confirmation text is exact.

## Validation Rules (Pre-check)
Pre-check validates before apply:
- ZIP structure (`manifest.json` + `database.sqlite`)
- manifest fields and supported `formatVersion`
- SHA-256 checksum consistency
- required table baseline
- SQLite `integrity_check` and `foreign_key_check`
- migration baseline compatibility vs current runtime

If incompatible, restore is not applied.

## Consistency and Failure Handling
- Backup/restore operations are serialized by a maintenance lock.
- Concurrent maintenance requests return `409 Conflict` with:
  - `reason = "OperationInProgress"`
- Restore apply is non-destructive on failure:
  - validation failure does not touch runtime data
  - apply failure attempts rollback to pre-restore snapshot

## Session Handling After Restore
Successful restore returns `RequiresReauthentication = true`.

The Web app clears current session/token and redirects to:
- `/login?reason=restore`

## Security Notes
- Endpoints require `CanWrite` (`Admin`) policy.
- Uploaded backup files are processed transiently and deleted after operation.
- Backup packages are not encrypted in this iteration.
- Backup files can contain sensitive financial and identity data; handle externally as sensitive data.

## Limitations
- SQLite-only implementation.
- No scheduled/automatic backups.
- No partial merge restore.
- No cloud storage integration.

## Rollback Guidance for Operators
- Always create a fresh backup before restore.
- If apply fails and rollback also fails, stop writes and restore from latest known-good `.ffbackup`.
- For severe recovery, replace runtime DB with a trusted backup and restart application services.
