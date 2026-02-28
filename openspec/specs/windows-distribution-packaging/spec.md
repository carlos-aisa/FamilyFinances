# windows-distribution-packaging Specification

## Purpose
TBD - created by archiving change windows-distribution-shared-binaries-layout. Update Purpose after archive.
## Requirements
### Requirement: Windows Distribution SHALL Use a Single Shared Runtime Root
The Windows ZIP distribution MUST place API and Web executables in one shared runtime root and MUST NOT include two full runtime trees that duplicate framework and dependency binaries.

#### Scenario: Build emits single-runtime layout
- **WHEN** the distribution build is executed for Windows (`win-x64`, self-contained)
- **THEN** the output root MUST contain both `FamilyFinances.Api.exe` and `FamilyFinances.Web.exe`
- **AND** the output MUST include exactly one physical copy for each identical shared runtime file
- **AND** the output MUST include `data/` and `logs/` sibling folders at distribution root

#### Scenario: Legacy duplicated runtime trees are not present
- **WHEN** the final distribution folder is inspected
- **THEN** it MUST NOT contain both `api/` and `web/` as full publish-runtime directories
- **AND** any remaining `api`/`web` naming in the layout MUST be limited to app-specific configuration context only

### Requirement: Packaging Merge SHALL Be Hash-Deterministic
The packaging process MUST use relative path + SHA-256 hash comparison to decide copy, dedup, redirect, or fail behavior.

#### Scenario: Identical file deduplication
- **WHEN** API and Web publish outputs contain the same relative path and the same SHA-256 hash
- **THEN** packaging MUST keep a single copy in the shared runtime root
- **AND** packaging MUST NOT duplicate that file under app-specific runtime trees

#### Scenario: Non-identical collision handling
- **WHEN** API and Web publish outputs contain the same relative path but different SHA-256 hashes
- **THEN** packaging MUST classify the file as either app-specific-config or unresolved-collision
- **AND** unresolved-collision files MUST fail the build before ZIP creation

### Requirement: App-Specific Configuration SHALL Remain Isolated
The distribution MUST isolate per-application configuration so API and Web config files cannot overwrite each other.

#### Scenario: Config files are separated by app
- **WHEN** packaging copies configuration assets from publish outputs
- **THEN** API config files MUST be placed under `config/api/`
- **AND** Web config files MUST be placed under `config/web/`
- **AND** each app config directory MUST include its own `appsettings.json` and `appsettings.Production.json`

#### Scenario: Packaged runtime resolves app-specific config deterministically
- **WHEN** API or Web process is started from packaged scripts
- **THEN** each process MUST load configuration from its own app-specific config directory
- **AND** local development behavior MUST remain unchanged when packaged-mode settings are not provided

### Requirement: Start Script SHALL Preserve Existing Operational UX
The packaged start command MUST keep current operator workflow while using the new single-runtime layout.

#### Scenario: Start script launches API then Web from shared root
- **WHEN** `Start FamilyFinances.bat` is executed
- **THEN** it MUST start `FamilyFinances.Api.exe` from shared runtime root
- **AND** it MUST wait for API health readiness before starting `FamilyFinances.Web.exe`
- **AND** it MUST open `http://localhost:5019` in the browser

#### Scenario: Start script retains port expectations
- **WHEN** packaged mode is started
- **THEN** API MUST be reachable at `http://localhost:5084`
- **AND** Web MUST be reachable at `http://localhost:5019`

### Requirement: Stop Script SHALL Stop Both Processes Reliably
The packaged stop command MUST reliably stop both runtime processes under the new layout.

#### Scenario: Stop script terminates API and Web
- **WHEN** `Stop FamilyFinances.bat` is executed
- **THEN** it MUST terminate `FamilyFinances.Api.exe` if running
- **AND** it MUST terminate `FamilyFinances.Web.exe` if running
- **AND** it MUST remove PID marker files created by the start process

### Requirement: CI Packaging SHALL Match Local Packaging Rules
The GitHub Actions distribution job MUST apply the same merge and verification rules as local distribution build.

#### Scenario: CI fails on unresolved collision
- **WHEN** CI packaging finds same-path files with different hashes that are not classified as app-specific configuration
- **THEN** CI MUST fail the packaging job before ZIP upload
- **AND** CI logs MUST list conflicting relative paths

#### Scenario: CI verifies required structure before ZIP upload
- **WHEN** CI packaging completes merge
- **THEN** CI MUST verify required files and folders for the new layout before creating/uploading ZIP
- **AND** CI MUST reject ZIP generation if required structure checks fail

### Requirement: Distribution Documentation SHALL Describe the New Layout
The operator-facing distribution README MUST document the new structure and runtime troubleshooting paths.

#### Scenario: README structure section matches packaged output
- **WHEN** README is reviewed after packaging changes
- **THEN** the documented folder tree MUST match the actual ZIP folder structure
- **AND** startup/stop instructions MUST remain accurate for the updated layout

### Requirement: Packaging Change SHALL Remain Behavior-Neutral for Product Features
This change MUST not alter business behavior, API contract behavior, or database schema behavior.

#### Scenario: Packaging-only scope enforcement
- **WHEN** implementation for this capability is complete
- **THEN** no API endpoint path/version or DTO schema MUST be changed by this capability
- **AND** no database migration files MUST be added by this capability
- **AND** any runtime code changes MUST be limited to packaged-mode configuration bootstrapping

