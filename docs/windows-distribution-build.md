# Windows Distribution Build Guide

This document explains how to build and publish the Windows installer-first distribution for FamilyFinances.

## Overview

The release flow now publishes:
- Primary artifact: setup bootstrapper (`*-setup.exe`) that installs Hosting Bundle if needed.
- Secondary artifact: raw MSI package (`*.msi`) with interactive install defaults and elevated host provisioning.

Installer mode provisions:
- `FamilyFinances.Web` in IIS.
- `FamilyFinances.Api` as Windows Service.
- Local-only default exposure with LAN opt-in.

## Building Locally

### Prerequisites
- .NET 9 SDK
- PowerShell 5.1 or later
- Windows 10/11 with administrative privileges for install actions

### Build command

```powershell
.\tools\installer\windows\build-installer.ps1 -Version "0.9.7" -Configuration Release
```

Output:
- `dist/FamilyFinances-v0.9.7-win-x64-msi-layout/`
- `dist/FamilyFinances-v0.9.7-win-x64-setup.exe`
- `dist/FamilyFinances-v0.9.7-win-x64.msi`

## MSI Source Layout

```text
FamilyFinances-v<version>-win-x64-msi-layout/
  constants.ps1
  FamilyFinances.Api.exe
  FamilyFinances.Web.exe
  config/api/*
  config/web/*
  ...
  installer-scripts/
    Assert-InstallerPreconditions.ps1
    Invoke-MsiConfigureInstall.ps1
    Invoke-MsiConfigureUninstall.ps1
    Set-ManagedRuntimeConfig.ps1
    Register-ApiService.ps1
    Configure-WebIisSite.ps1
    Set-LanAccess.ps1
    ...
```

## Install and Uninstall

### Install

Install with defaults:

```powershell
.\dist\FamilyFinances-v0.9.7-win-x64-setup.exe
```

Install with parameter overrides:

```powershell
msiexec /i .\dist\FamilyFinances-v0.9.7-win-x64.msi INSTALLDIR="C:\Program Files\FamilyFinances" RUNTIMEROOT="C:\ProgramData\FamilyFinances" ENABLEIISIFMISSING=1
```

Notes:
- The setup bootstrapper performs a web download of .NET 9 Hosting Bundle only when `AspNetCoreModuleV2` is missing.
- Use raw MSI only for advanced/manual scenarios where prerequisites are already handled.

Uninstall (data preserved by default):

```powershell
msiexec /x .\dist\FamilyFinances-v0.9.7-win-x64.msi
```

Uninstall with runtime data cleanup:

```powershell
msiexec /x .\dist\FamilyFinances-v0.9.7-win-x64.msi MSIREMOVEDATA=1
```

## GitHub Actions Release Flow

Windows release packaging is handled by:
- `.github/workflows/release-windows.yml`

Trigger policy:
- Automatic packaging and release publish on push to `main`.
- Workflow computes the next patch tag (`vX.Y.Z`) from existing semantic tags.
- Optional manual cleanup path via `workflow_dispatch`.

Release workflow behavior:
1. Pre-clean old managed release assets before publishing.
2. Build installer package (`tools/installer/windows/build-installer.ps1`).
3. Validate setup bootstrapper and MSI artifacts exist.
4. Publish setup bootstrapper first and MSI second.

Retention behavior:
- Cleanup targets managed installer assets:
  - `FamilyFinances-v*-win-x64-setup.exe`
  - `FamilyFinances-v*-win-x64.msi`
- Pre-clean keeps only 2 previous releases for managed asset patterns.
- Legacy ZIP cleanup pattern may remain for historical assets, but new releases are installer-only.

## Security Baseline

- API binding is loopback-only in production installer mode.
- Local-only access is default.
- LAN access is opt-in and HTTPS-only.
- LAN firewall allowance is private profile only.
- Runtime JWT signing key is generated per installation when missing/default.

## Rollback Guidance

If installer rollout has issues:
1. Keep runtime data path intact for continuity and backup validation.
2. Run installer repair/reinstall using the same MSI line.
3. Preserve runtime data path for recovery/migration.

## Troubleshooting

### Install fails on prerequisite checks
- Run PowerShell as Administrator.
- Ensure IIS features can be enabled on the host.
- Verify `dotnet --info` works.

### Service or IIS startup failures
- Verify Windows Service `FamilyFinances.Api` status.
- Verify IIS site/app pool `FamilyFinances.Web` status.
- Check runtime logs under managed runtime root.

### LAN access issues
- Confirm LAN mode is enabled in `/settings`.
- Confirm private-profile firewall rule exists for configured HTTPS port.
- Confirm mobile device trusts the locally generated root certificate.
