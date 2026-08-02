## MODIFIED Requirements

### Requirement: Installer SHALL Provision Web and API Runtime Topology
The Windows installer MUST converge IIS and ASP.NET Core IIS hosting prerequisites before provisioning `FamilyFinances.Web` under IIS and `FamilyFinances.Api` as a Windows Service configured for automatic startup.

#### Scenario: Fresh install on a clean host converges prerequisites before provisioning
- **WHEN** a user runs the supported setup bootstrapper on a machine where required IIS features are disabled or `AspNetCoreModuleV2` is not registered
- **THEN** the installer MUST enable the required IIS Windows features before final ASP.NET Core IIS module validation
- **AND** the installer MUST install or repair the Hosting Bundle after IIS becomes available when `AspNetCoreModuleV2` is still missing
- **AND** the installer MUST continue to IIS site and API service provisioning only after `AspNetCoreModuleV2` is registered successfully

#### Scenario: Retry after partial prerequisite convergence remains safe
- **WHEN** a prior install attempt already enabled some IIS features or partially completed Hosting Bundle setup
- **THEN** a retry or repair run MUST re-evaluate prerequisite state idempotently
- **AND** the installer MUST avoid duplicating or corrupting existing prerequisite registrations while converging to the required state

### Requirement: Installer Completion SHALL Require Health Verification
Installer success criteria MUST include prerequisite convergence and runtime validation checks before reporting completion.

#### Scenario: Install stops with actionable guidance when prerequisite convergence requires restart
- **WHEN** prerequisite convergence cannot complete in the current session because Windows reports a restart-required or pending-reboot state
- **THEN** the installer MUST stop before IIS site or API service provisioning begins
- **AND** the installer MUST report that a reboot is required before setup is rerun

#### Scenario: Install fails with prerequisite root-cause diagnostics
- **WHEN** the installer cannot converge required IIS or Hosting Bundle prerequisites
- **THEN** the installer MUST report which prerequisite category failed (for example IIS feature activation, `AspNetCoreModuleV2` registration, or Hosting Bundle repair)
- **AND** the installer MUST NOT report successful completion
