# Windows MSI Installer Design (Installer-First + ZIP Fallback)

Date: 2026-03-15
Status: Approved and implemented

## Scope

Replace the current installer ZIP as primary release artifact with a real MSI package while keeping runtime ZIP as fallback during transition.

## Architecture

- Packaging remains driven by `tools/installer/windows/build-installer.ps1`.
- Runtime payload is still produced by `build-windows-dist.ps1`.
- New MSI source layout is produced by `tools/installer/windows/scripts/Publish-MsiLayout.ps1`.
- WiX v4 project (`tools/installer/windows/wix/FamilyFinances.Installer.wixproj`) compiles MSI from the prepared layout.
- WiX authoring (`FamilyFinances.Installer.wxs`) uses:
  - `WixUI_InstallDir` for interactive install UX.
  - deferred elevated custom actions to run host provisioning scripts.

## Components

- New MSI scripts:
  - `Invoke-MsiConfigureInstall.ps1`: prechecks, managed runtime config/JWT hardening, API service registration, IIS site provisioning, health checks.
  - `Invoke-MsiConfigureUninstall.ps1`: LAN disable cleanup, service/site removal, optional runtime data cleanup (`MSIREMOVEDATA=1`).
- Existing host operation scripts are reused under `installer-scripts/`.

## Data and Configuration Flow

1. Build creates app runtime dist folder and fallback ZIP.
2. MSI layout copies dist payload + installer scripts.
3. MSI installs files to `INSTALLDIR` (interactive default path).
4. MSI deferred install action runs `Invoke-MsiConfigureInstall.ps1`.
5. Script writes managed runtime config under runtime root (`FF_RUNTIME_ROOT`) and configures host topology.
6. On uninstall, deferred uninstall action runs cleanup script before file removal.

## Error Handling

- MSI custom actions run with `Return="check"` for install path and fail setup on provisioning errors.
- Build script validates WiX exit code and verifies MSI output exists.
- Release workflow validates both MSI and ZIP artifact existence before publishing.

## Release and Retention

- Primary artifact: `FamilyFinances-v<version>-win-x64.msi`.
- Fallback artifact: `FamilyFinances-v<version>-win-x64.zip`.
- Release cleanup rules now include MSI pattern plus ZIP fallback pattern.

## Testing Strategy

- Verified MSI build generation via `build-installer.ps1`.
- Verified regression safety with `dotnet test -c Release`.
- Manual smoke (install/reboot/LAN toggle/uninstall) remains required as operational validation gate.

