# windows-installer-managed-deployment Specification

## Purpose
Define installer-managed deployment topology and lifecycle for Windows home-user installations.

## Requirements
### Requirement: Installer SHALL Provision Web and API Runtime Topology
The Windows installer MUST converge IIS and ASP.NET Core IIS hosting prerequisites before provisioning `FamilyFinances.Web` under IIS and `FamilyFinances.Api` as a Windows Service configured for automatic startup.

#### Scenario: Fresh install provisions IIS site and API service
- **WHEN** a user performs a fresh installation with administrative privileges
- **THEN** the installer MUST create and configure an IIS site/app pool for `FamilyFinances.Web`
- **AND** the installer MUST register `FamilyFinances.Api` as a Windows Service with automatic startup mode

#### Scenario: Installed runtime starts automatically after reboot
- **WHEN** the machine reboots after successful installation
- **THEN** IIS and the API Windows Service MUST start without manual `.bat` execution
- **AND** the local web entrypoint MUST become reachable when runtime dependencies are healthy

#### Scenario: Fresh install on a clean host converges prerequisites before provisioning
- **WHEN** a user runs the supported setup bootstrapper on a machine where required IIS features are disabled or `AspNetCoreModuleV2` is not registered
- **THEN** the installer MUST enable the required IIS Windows features before final ASP.NET Core IIS module validation
- **AND** the installer MUST install or repair the Hosting Bundle after IIS becomes available when `AspNetCoreModuleV2` is still missing
- **AND** the installer MUST continue to IIS site and API service provisioning only after `AspNetCoreModuleV2` is registered successfully

#### Scenario: Retry after partial prerequisite convergence remains safe
- **WHEN** a prior install attempt already enabled some IIS features or partially completed Hosting Bundle setup
- **THEN** a retry or repair run MUST re-evaluate prerequisite state idempotently
- **AND** the installer MUST avoid duplicating or corrupting existing prerequisite registrations while converging to the required state

### Requirement: Installer SHALL Configure Managed Runtime Paths and Configuration
The installer MUST place binaries in managed installation directories and MUST isolate mutable runtime state (config, data, logs) in managed writable locations.

#### Scenario: Managed folder layout is applied
- **WHEN** installation completes successfully
- **THEN** executable payload files MUST exist in installer-managed program directories
- **AND** runtime configuration, data, and logs MUST be stored outside binary directories in writable managed paths

#### Scenario: Runtime config root is deterministic
- **WHEN** Web and API processes start in installed mode
- **THEN** each process MUST resolve its packaged configuration from installer-managed config roots
- **AND** environment variable overrides MUST remain supported

### Requirement: Installer SHALL Generate Installation-Specific Secrets
The installer MUST ensure production deployments do not use default static secrets by generating installation-specific JWT signing material when missing or default.

#### Scenario: Default JWT key is replaced during install
- **WHEN** installer detects a missing or known-default JWT key value
- **THEN** installer MUST generate a new installation-specific key meeting minimum security constraints
- **AND** runtime configuration MUST be updated before API startup

#### Scenario: Existing custom JWT key is preserved on upgrade
- **WHEN** installer performs an upgrade and an existing non-default key is present
- **THEN** installer MUST preserve the existing key by default
- **AND** upgrade MUST continue without forced secret rotation

### Requirement: Installer SHALL Support Upgrade and Uninstall Lifecycle
The deployment model MUST support in-place upgrades and uninstall behavior with data-preservation defaults.

#### Scenario: Upgrade keeps user data and service topology
- **WHEN** a new installer version upgrades an existing installation
- **THEN** installer MUST preserve existing user data and configuration unless explicit override is requested
- **AND** IIS site and API service registrations MUST remain valid after upgrade

#### Scenario: Uninstall preserves data by default
- **WHEN** user uninstalls the product without selecting destructive cleanup
- **THEN** executable/service/site registrations MUST be removed
- **AND** user data and backup files MUST remain available for recovery

### Requirement: Installer Completion SHALL Require Health Verification
Installer success criteria MUST include prerequisite convergence and runtime validation checks before reporting completion.

#### Scenario: Install fails when runtime health verification fails
- **WHEN** post-install validation cannot reach required runtime health endpoints
- **THEN** installer MUST report installation failure with actionable diagnostics
- **AND** installer MUST NOT report successful completion status

#### Scenario: Install succeeds only after health checks pass
- **WHEN** post-install validation reaches required runtime health endpoints successfully
- **THEN** installer MUST report successful completion
- **AND** the installed application MUST be immediately usable in local-only mode

#### Scenario: Install stops with actionable guidance when prerequisite convergence requires restart
- **WHEN** prerequisite convergence cannot complete in the current session because Windows reports a restart-required or pending-reboot state
- **THEN** the installer MUST stop before IIS site or API service provisioning begins
- **AND** the installer MUST report that a reboot is required before setup is rerun

#### Scenario: Install fails with prerequisite root-cause diagnostics
- **WHEN** the installer cannot converge required IIS or Hosting Bundle prerequisites
- **THEN** the installer MUST report which prerequisite category failed (for example IIS feature activation, `AspNetCoreModuleV2` registration, or Hosting Bundle repair)
- **AND** the installer MUST NOT report successful completion
