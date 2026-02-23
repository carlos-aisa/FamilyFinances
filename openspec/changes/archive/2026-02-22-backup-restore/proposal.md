## Why

Users need a safe and predictable way to preserve their data, recover from mistakes, and move the application between machines without manual database/file handling. This is now required to close the `0.9.x` usability and operational baseline.

## What Changes

- Add a first-class Backup/Restore feature reachable from the Settings area of the Web app.
- Add backup export flow for the full application data scope required to reconstruct a working instance.
- Add restore flow with explicit pre-checks (format/version compatibility and integrity) before applying data.
- Ensure restore execution is deterministic and protects consistency (all-or-nothing apply path).
- Add operator-facing feedback for successful/failed backup and restore operations.
- Add automated tests and documentation for backup/restore behavior, limits, and safety constraints.

### Non-Goals

- No scheduled/automatic cloud backups in this iteration.
- No cross-currency or cross-schema transformation tooling beyond explicit compatibility validation.
- No partial merge restore strategy; this change focuses on full restore semantics.
- No redesign of reporting/business metrics as part of backup/restore.

### Rollback Plan

- Keep Backup/Restore surfaced as additive functionality so core ledger/reporting flows remain unchanged if disabled.
- Guard restore execution behind explicit validation checks; if regressions are detected, disable restore entry points while keeping backup export available.
- Revert UI entry points and API wiring without requiring data model rollback for unaffected features.
- Retain existing runtime data untouched when restore pre-checks fail.

## Capabilities

### New Capabilities

- `backup-restore`: End-to-end backup export and restore workflows, including validation, safety constraints, and user-visible outcomes.

### Modified Capabilities

- `system`: Extend baseline requirements with operational data protection and recovery behavior (backup/restore availability and safety guarantees).

## Impact

- Web: new Settings interactions for backup export and restore initiation/confirmation.
- API/Application: backup package generation, restore validation, and atomic apply orchestration.
- Infrastructure: persistence-level snapshot/restore implementation and compatibility checks.
- Testing: new integration and UI tests for success paths, invalid packages, and rollback-safe behavior.
- Documentation: user/admin guidance for backup/restore procedure, constraints, and failure handling.
