# Windows Installer Toolkit

This folder contains the installer-first deployment toolkit for FamilyFinances on Windows.

## Build

From repository root:

```powershell
.\tools\installer\windows\build-installer.ps1 -Version "0.9.7" -Configuration Release
```

Outputs:
- `dist/FamilyFinances-v<version>-win-x64-msi-layout/`
- `dist/FamilyFinances-v<version>-win-x64-setup.exe` (primary bootstrapper)
- `dist/FamilyFinances-v<version>-win-x64.msi` (raw MSI artifact)

Note:
- `*-setup.exe` downloads .NET 9 Hosting Bundle from `aka.ms` only when `AspNetCoreModuleV2` is missing.

## Install and uninstall

```powershell
.\dist\FamilyFinances-v<version>-win-x64-setup.exe
msiexec /i .\dist\FamilyFinances-v<version>-win-x64.msi
msiexec /x .\dist\FamilyFinances-v<version>-win-x64.msi
```
