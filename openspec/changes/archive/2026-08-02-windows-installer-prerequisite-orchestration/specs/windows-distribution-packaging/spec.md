## MODIFIED Requirements

### Requirement: Release Packaging SHALL Be Installer-Only
The release flow MUST publish installer assets (`*-setup.exe`, `*.msi`) as the only active distribution artifacts, and the published setup bootstrapper MUST remain the supported self-provisioning entrypoint for home-user installs.

#### Scenario: Setup bootstrapper supports clean-host prerequisite convergence
- **WHEN** a user launches the published `*-setup.exe` on a Windows host that may not already have IIS or `AspNetCoreModuleV2` available
- **THEN** the setup artifact MUST coordinate the prerequisite convergence required for one-click installation
- **AND** the user MUST NOT be required to manually enable IIS or rerun the Hosting Bundle as a normal first-install step

#### Scenario: Raw MSI remains an advanced/manual artifact
- **WHEN** the Windows distribution is documented or presented to maintainers/users
- **THEN** the raw MSI MUST remain identified as the advanced/manual artifact
- **AND** the setup bootstrapper MUST remain the recommended path for clean-machine installation

### Requirement: Distribution Documentation SHALL Describe Installer-Only Layout
Distribution documentation MUST reflect installer-first lifecycle, installer-only release artifacts, prerequisite convergence behavior, and recovery guidance.

#### Scenario: Documentation includes clean-host prerequisite guidance
- **WHEN** distribution documentation is updated for installer releases
- **THEN** it MUST explain how the supported setup flow handles IIS activation and Hosting Bundle convergence
- **AND** it MUST include troubleshooting guidance for restart-required or prerequisite-repair failures
