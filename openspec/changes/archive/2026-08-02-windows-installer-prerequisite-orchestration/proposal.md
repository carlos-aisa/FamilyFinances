## Why

Fresh Windows installs can fail in the supported `*-setup.exe` flow when IIS is not already enabled. The current sequence installs the .NET Hosting Bundle before IIS is activated, which leaves `AspNetCoreModuleV2` unavailable and causes the MSI to fail with a generic `0x80070643` instead of completing the promised one-click setup experience.

## What Changes

- Rework Windows installer prerequisite orchestration so the supported setup flow converges IIS features and ASP.NET Core IIS hosting prerequisites before provisioning the site and service.
- Add prerequisite self-healing so the installer can rerun or repair Hosting Bundle registration after IIS activation when `AspNetCoreModuleV2` is still missing.
- Stage the .NET Hosting Bundle payload inside the MSI layout so install-time prerequisite repair does not depend on a second manual download path.
- Add explicit pending-reboot and prerequisite-failure diagnostics so install failures report actionable next steps instead of a generic MSI error.
- Preserve the existing runtime topology (`FamilyFinances.Web` in IIS, `FamilyFinances.Api` as a Windows Service) while tightening only the prerequisite phase and installer guidance.
- Update installer validation coverage and Windows distribution documentation for clean-host setup, restart guidance, and troubleshooting.

### Non-Goals

- No change to the deployed runtime topology, LAN settings behavior, or API exposure model.
- No migration away from IIS plus Windows Service hosting.
- No introduction of non-Windows installer targets or public internet deployment scenarios.
- No redesign of application features unrelated to installer prerequisite handling.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `windows-installer-managed-deployment`: Tighten installer requirements so clean hosts without IIS or `AspNetCoreModuleV2` are converged before provisioning and failures report actionable diagnostics.
- `windows-distribution-packaging`: Clarify that the published setup bootstrapper is the supported self-provisioning entrypoint for home-user installs and update documentation/troubleshooting expectations accordingly.

## Impact

- Affected installer code:
  - `tools/installer/windows/bootstrapper/*`
  - `tools/installer/windows/scripts/*`
  - `tools/installer/windows/build-installer.ps1`
  - `tools/installer/windows/scripts/Publish-MsiLayout.ps1`
- Affected packaged installer layout:
  - `dist/FamilyFinances-v<version>-win-x64-msi-layout/installer-prereqs/*`
- Affected documentation:
  - `tools/installer/windows/README.md`
  - `docs/windows-distribution-build.md`
  - Potentially `README.md` if install guidance changes at the top level.
- Affected validation:
  - Installer smoke validation on a clean Windows host without IIS pre-enabled.
  - Focused regression validation for prerequisite convergence and failure diagnostics.
- APIs/data model:
  - No API contract changes.
  - No database or migration changes.

## Release Impact

Type: patch
Rationale: Fixes a backward-compatible installer defect that blocks clean-machine installation without changing application features or public contracts.

## Rollback Plan

- Revert the prerequisite orchestration changes in bootstrapper/MSI scripts and restore the previous setup sequencing.
- Revert any added prerequisite diagnostics/docs to the prior manual troubleshooting guidance.
- If rollback is required after release, communicate that affected users must enable IIS and rerun or repair the Hosting Bundle manually before reinstalling.
