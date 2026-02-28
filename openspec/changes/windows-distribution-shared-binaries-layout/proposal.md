## CRITICAL IMPLEMENTATION CONSTRAINTS

- Do not change API routes, DTO contracts, authentication behavior, or database schema.
- Do not remove `Start FamilyFinances.bat` or `Stop FamilyFinances.bat`; they must remain the entry point.
- Do not merge runtime outputs into one folder unless assembly/version conflicts are eliminated first.
- Ensure API and Web can run simultaneously from the packaged distribution without file overwrite races.
- Keep distribution as a single ZIP artifact.

## Why

The current Windows distribution duplicates a large set of runtime files because API and Web are published into separate folders with mostly overlapping dependencies. This unnecessarily increases ZIP size and GitHub artifact/release storage costs.

## What Changes

- Introduce a single-binary-layout packaging capability for Windows distribution where shared runtime files are stored once.
- Keep both startup scripts at ZIP root, but package API and Web with a conflict-safe layout that preserves each app's configuration and startup context.
- Add deterministic packaging rules to prevent overwriting files that have the same name but different content/version.
- Update local packaging script and GitHub workflow packaging steps to produce the new layout consistently.
- Add packaging verification checks (required files, startup expectations, conflict checks) before ZIP creation.
- Update distribution documentation to describe the new folder layout and operational behavior.

### Non-goals

- No change to API or Web functional features.
- No change to endpoint paths, authorization policies, or business logic.
- No change to backup/restore functional behavior.
- No migration to a different distribution format (still ZIP).
- No Linux/macOS distribution redesign in this change.

### Rollback Plan

- Revert packaging script and CI workflow steps to the previous `api/` + `web/` full-copy layout.
- Restore previous distribution README structure and script directory assumptions.
- Rebuild and republish the prior ZIP format from the same tagged source.
- Keep application code behavior unchanged so rollback is packaging-only.

## Capabilities

### New Capabilities
- `windows-distribution-packaging`: Defines conflict-safe single-layout Windows ZIP packaging with shared runtime binaries and preserved app startup/config contexts.

### Modified Capabilities
- None.

## Impact

- Affected local packaging script: `D:/Programacion/FamilyFinances/build-windows-dist.ps1`.
- Affected runtime launch scripts and docs: `D:/Programacion/FamilyFinances/dist/Start FamilyFinances.bat`, `D:/Programacion/FamilyFinances/dist/Stop FamilyFinances.bat`, `D:/Programacion/FamilyFinances/dist/README.txt`.
- Affected CI distribution packaging: `D:/Programacion/FamilyFinances/.github/workflows/ci.yml`.
- Potentially affected dependency/version alignment inputs in project/package configuration if required to avoid runtime collisions.
