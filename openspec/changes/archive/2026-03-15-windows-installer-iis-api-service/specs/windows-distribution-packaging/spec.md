## MODIFIED Requirements

### Requirement: Release Packaging SHALL Be Installer-Only
The release flow MUST publish installer assets (`*-setup.exe`, `*.msi`) as the only active distribution artifacts.

#### Scenario: Release publishes setup bootstrapper and MSI only
- **WHEN** release packaging runs successfully
- **THEN** the workflow MUST publish setup bootstrapper and raw MSI artifacts
- **AND** release metadata MUST identify installer package version

#### Scenario: Runtime ZIP is not published in active flow
- **WHEN** release assets are generated for a new version
- **THEN** runtime ZIP artifacts MUST NOT be published as active deliverables
- **AND** legacy ZIP cleanup patterns MAY remain only for historical asset cleanup

### Requirement: CI Packaging SHALL Use Main-Push Gating With Auto Versioning
The release packaging workflow MUST run on pushes to `main` and MUST compute the next semantic release tag automatically.

#### Scenario: Packaging does not run from non-main branch pushes
- **WHEN** a workflow run is triggered by a branch push outside `main`
- **THEN** release packaging MUST NOT execute

#### Scenario: Packaging on main computes and publishes semantic version
- **WHEN** a workflow run is triggered by push to `main`
- **THEN** the workflow MUST compute the next semantic version tag
- **AND** publish installer assets under that tag

### Requirement: Release Asset Cleanup SHALL Execute Before Publish
Release cleanup MUST run before publish and retain only configured historical managed assets.

#### Scenario: Pre-clean executes before release publish
- **WHEN** release packaging starts for a new version
- **THEN** cleanup MUST run before publish
- **AND** only configured count of historical managed assets MUST be kept

#### Scenario: Manual cleanup remains available
- **WHEN** maintainers trigger workflow dispatch cleanup
- **THEN** managed release assets older than configured keep-count MUST be deleted
- **AND** recent managed assets MUST remain available

### Requirement: Distribution Documentation SHALL Describe Installer-Only Layout
Distribution documentation MUST reflect installer-first lifecycle, installer-only release artifacts, LAN security defaults, and recovery guidance.

#### Scenario: Documentation prioritizes installer lifecycle guidance
- **WHEN** distribution documentation is updated for this capability
- **THEN** install, upgrade, uninstall, and automatic startup behavior MUST be documented as default operational path
- **AND** installer-only artifact policy MUST be explicit

#### Scenario: Documentation includes LAN and certificate trust guidance
- **WHEN** LAN mode is supported for installer deployments
- **THEN** documentation MUST describe secure defaults and LAN opt-in behavior
- **AND** documentation MUST include mobile trust/import guidance for locally generated certificates
