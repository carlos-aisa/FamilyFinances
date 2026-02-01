# Windows Distribution Build Guide

This document explains how to build and publish the Windows ZIP distribution for FamilyFinances v0.6.7.

## Overview

The Windows distribution provides a self-contained, portable version of FamilyFinances that non-technical users can run without installing .NET or any other dependencies.

## Distribution Structure

```
FamilyFinances-v0.6.7-win-x64/
  ├── Start FamilyFinances.bat     # Launcher script
  ├── Stop FamilyFinances.bat      # Shutdown script
  ├── README.txt                   # End-user documentation
  ├── data/                        # SQLite database (created on first run)
  ├── logs/                        # Application logs
  ├── api/                         # API binaries
  │   ├── FamilyFinances.Api.exe
  │   ├── appsettings.Production.json
  │   └── ... (runtime files)
  └── web/                         # Web binaries
      ├── FamilyFinances.Web.exe
      ├── appsettings.Production.json
      └── ... (runtime files)
```

## Building Locally

### Prerequisites
- .NET 9.0 SDK
- PowerShell 5.1 or later
- Windows 10/11

### Steps

1. **Run the build script:**
   ```powershell
   .\build-windows-dist.ps1
   ```

2. **Optional: Specify version:**
   ```powershell
   .\build-windows-dist.ps1 -Version "0.6.7"
   ```

3. **Output:**
   - Distribution folder: `dist/FamilyFinances-v0.6.7-win-x64/`
   - ZIP archive: `dist/FamilyFinances-v0.6.7-win-x64.zip`

### Testing Locally

1. Navigate to the distribution folder:
   ```powershell
   cd dist\FamilyFinances-v0.6.7-win-x64
   ```

2. Run the start script:
   ```cmd
   Start FamilyFinances.bat
   ```

3. Test the application at `http://localhost:5019`

4. Stop the application:
   ```cmd
   Stop FamilyFinances.bat
   ```

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci.yml`) automatically builds the Windows distribution on:
- Pushes to `main` or `develop` branches
- Pull requests to `main`
- Version tags (e.g., `v0.6.7`)

### Workflow Steps

1. **Build and Test** (Ubuntu)
   - Restore dependencies
   - Build solution
   - Run all tests

2. **Windows Distribution** (Windows)
   - Publish API as self-contained win-x64
   - Publish Web as self-contained win-x64
   - Assemble distribution folder
   - Verify required files
   - Create ZIP archive
   - Upload as artifact
   - *If tagged:* Create GitHub Release

### Downloading Artifacts

After a successful CI run:
1. Go to the Actions tab in GitHub
2. Select the workflow run
3. Download the ZIP from the Artifacts section

### Creating a Release

To create a GitHub Release:

1. Tag the commit:
   ```bash
   git tag v0.6.7
   git push origin v0.6.7
   ```

2. The workflow will automatically:
   - Build the distribution
   - Create a GitHub Release
   - Attach the ZIP file

## Configuration

### Production Settings

**API (`src/FamilyFinances.Api/appsettings.Production.json`):**
- Database: `../data/familyfinances.db` (portable)
- Logs: `../logs/api-YYYYMMDD.log` (daily rotation)
- Port: `5084` (HTTP only)
- JWT: Uses default key (should be changed for production deployments)

**Web (`src/FamilyFinances.Web/appsettings.Production.json`):**
- API URL: `http://localhost:5084/`
- Port: `5019` (HTTP only)

### Startup Behavior

`Start FamilyFinances.bat`:
1. Creates `data/` and `logs/` folders
2. Starts API in minimized window
3. Polls API health endpoint (30 second timeout)
4. Starts Web in minimized window
5. Opens browser to `http://localhost:5019`
6. Saves PIDs to `.pid` files for shutdown

### Shutdown Behavior

`Stop FamilyFinances.bat`:
1. Terminates Web process
2. Terminates API process
3. Cleans up `.pid` files

## Publish Settings

Both projects use:
- `--runtime win-x64`
- `--self-contained true`
- `PublishTrimmed=false` (safer, larger size)
- `PublishSingleFile=false` (better compatibility)

## Troubleshooting

### Build fails with "not found" errors
- Ensure .NET 9.0 SDK is installed: `dotnet --version`
- Run `dotnet restore` manually

### Distribution won't start
- Check ports 5084 and 5019 aren't in use
- Review logs in `logs/api-*.log`
- Ensure no firewall is blocking the executables

### Database issues
- Database is created automatically on first run
- Located at `data/familyfinances.db`
- Backup by copying the entire `data/` folder

## Future Improvements

Consider for future versions:
- Single-file publish (if compatible with all dependencies)
- Configurable ports via environment variables
- Installer (MSI) instead of ZIP
- Auto-update mechanism
- HTTPS with self-signed certificate option
