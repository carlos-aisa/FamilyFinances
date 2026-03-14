# Windows Distribution Build Guide

This document explains how to build and publish the Windows ZIP distribution for FamilyFinances.

## Overview

The Windows ZIP distribution is a self-contained package that allows users to run the app without installing .NET separately.

## Distribution Structure (Shared Runtime Layout)

```text
FamilyFinances-v<version>-win-x64/
  Start FamilyFinances.bat
  Stop FamilyFinances.bat
  README.txt
  FamilyFinances.Api.exe
  FamilyFinances.Web.exe
  *.dll / *.deps.json / *.runtimeconfig.json
  config/
    api/appsettings.json
    api/appsettings.Production.json
    web/appsettings.json
    web/appsettings.Production.json
  data/
  logs/
  wwwroot/
```

## Building Locally

### Prerequisites
- .NET 9 SDK
- PowerShell 5.1 or later
- Windows 10/11

### Build command

```powershell
.\build-windows-dist.ps1 -Version "0.6.7" -Configuration Release
```

Output:
- Folder: `dist/FamilyFinances-v0.6.7-win-x64/`
- ZIP: `dist/FamilyFinances-v0.6.7-win-x64.zip`

### Local smoke check

```cmd
cd dist\FamilyFinances-v0.6.7-win-x64
Start FamilyFinances.bat
Stop FamilyFinances.bat
```

## GitHub Actions Release Flow

Windows release packaging is handled by:
- `.github/workflows/release-windows.yml`

Trigger policy:
- Automatic packaging and release publish only on version tags `v*.*.*`.
- Optional manual cleanup path via `workflow_dispatch`.

Release workflow behavior:
1. Pre-clean old release ZIP assets before publishing (keep latest 2 historical ZIPs).
2. Build and validate distribution.
3. Run ZIP smoke test.
4. Upload ZIP artifact (short retention).
5. Create/update GitHub Release and attach ZIP.

Retention behavior:
- Cleanup targets ZIP assets matching `FamilyFinances-v*-win-x64.zip`.
- Pre-clean keeps only 2 previous ZIP assets.
- After publishing the new ZIP, the recent set is expected to be 3 total (new + two previous).

## Creating a Release

```bash
git tag v0.6.7
git push origin v0.6.7
```

The release workflow will package and publish the ZIP automatically.

## Runtime Configuration Notes

API production config:
- `src/FamilyFinances.Api/appsettings.Production.json`

Web production config:
- `src/FamilyFinances.Web/appsettings.Production.json`

Packaged runtime config files are isolated under:
- `config/api/*`
- `config/web/*`

## Troubleshooting

### Build fails
- Verify SDK: `dotnet --version`
- Run `dotnet restore` before building.

### Distribution start issues
- Check ports `5084` and `5019`.
- Check logs under `logs/`.
- Confirm executables are not blocked by local security policies.

### Database path issues
- Packaged mode writes runtime DB under `%LOCALAPPDATA%\FamilyFinances\data\` through startup script configuration.
