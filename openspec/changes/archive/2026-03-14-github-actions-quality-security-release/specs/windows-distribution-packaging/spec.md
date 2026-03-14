## MODIFIED Requirements

### Requirement: CI Packaging SHALL Match Local Packaging Rules
The GitHub Actions distribution job MUST apply the same merge and verification rules as local distribution build, MUST run only for version tag releases, and MUST enforce release ZIP retention cleanup before publish.

#### Scenario: CI fails on unresolved collision
- **WHEN** CI packaging finds same-path files with different hashes that are not classified as app-specific configuration
- **THEN** CI MUST fail the packaging job before ZIP upload
- **AND** CI logs MUST list conflicting relative paths

#### Scenario: CI verifies required structure before ZIP upload
- **WHEN** CI packaging completes merge
- **THEN** CI MUST verify required files and folders for the new layout before creating/uploading ZIP
- **AND** CI MUST reject ZIP generation if required structure checks fail

#### Scenario: Packaging release workflow runs only for version tags
- **WHEN** a workflow run is triggered by a branch push to `main` or `develop`
- **THEN** Windows distribution packaging MUST NOT run
- **AND** packaging MUST run when a `v*.*.*` version tag push occurs

#### Scenario: ZIP cleanup executes before new release publish
- **WHEN** a `v*.*.*` release tag workflow starts packaging
- **THEN** release ZIP cleanup MUST execute before creating or publishing the new ZIP
- **AND** cleanup MUST target only assets matching `FamilyFinances-v*-win-x64.zip`

#### Scenario: ZIP retention keeps only two prior assets before publish
- **WHEN** release ZIP cleanup executes before publish
- **THEN** the cleanup MUST retain only the two most recent historical matching ZIP assets
- **AND** after publishing the new ZIP, the recent matching ZIP set MUST be exactly three assets (new + two previous)

