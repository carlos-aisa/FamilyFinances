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
- The MSI layout now stages `installer-prereqs/dotnet-hosting-9.0-win.exe` so install-time prerequisite repair can re-register `AspNetCoreModuleV2` after IIS is enabled.

## Install and uninstall

```powershell
.\dist\FamilyFinances-v<version>-win-x64-setup.exe
msiexec /i .\dist\FamilyFinances-v<version>-win-x64.msi
msiexec /x .\dist\FamilyFinances-v<version>-win-x64.msi
```

Recommended path:
- Use `*-setup.exe` for clean Windows machines where IIS may not already be enabled.
- Keep raw MSI for advanced/manual scenarios. It now carries the Hosting Bundle payload for prerequisite repair, but the bootstrapper remains the default home-user entrypoint.

Troubleshooting:
- If setup reports that Windows restart is required before IIS prerequisites can finish, reboot the machine and rerun the installer.
- If the installer still cannot register `AspNetCoreModuleV2`, rerun the same setup package so it can retry Hosting Bundle maintenance with the staged prerequisite payload.
