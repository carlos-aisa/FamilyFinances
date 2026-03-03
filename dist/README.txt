================================================================================
                            FAMILYFINANCES v0.6.7
                         Windows Standalone Edition
================================================================================

WELCOME

Thank you for using FamilyFinances. This is a portable self-contained version
that runs locally on your computer. No installation or shared .NET runtime is
required.

================================================================================
QUICK START
================================================================================

1. START THE APPLICATION

   Double-click:  Start FamilyFinances.bat

   This script will:
   - Create required folders (logs and local data path)
   - Start API and Web services
   - Open your browser automatically

   URL: http://localhost:5019

2. USE THE APPLICATION

   - Create your account on first use
   - Data is stored in: %LOCALAPPDATA%\FamilyFinances\data\
   - Services run locally and are not exposed externally by default

3. STOP THE APPLICATION

   Double-click:  Stop FamilyFinances.bat

   This script stops both services and cleans PID files.

================================================================================
FILE STRUCTURE
================================================================================

FamilyFinances-v0.6.7-win-x64/
  |
  |-- Start FamilyFinances.bat
  |-- Stop FamilyFinances.bat
  |-- README.txt
  |
  |-- FamilyFinances.Api.exe
  |-- FamilyFinances.Web.exe
  |-- *.dll / *.deps.json / *.runtimeconfig.json
  |   (shared runtime files, single copy)
  |
  |-- config/
  |   |-- api/
  |   |   |-- appsettings.json
  |   |   |-- appsettings.Production.json
  |   |   `-- web.config
  |   `-- web/
  |       |-- appsettings.json
  |       |-- appsettings.Production.json
  |       `-- web.config
  |
  |-- wwwroot/                  (Web static assets)
  |-- en-US/                    (Web resources)
  |-- es-ES/                    (Web resources)
  |
  |-- data/
  |   `-- (optional fallback path if LOCALAPPDATA is unavailable)
  |
  `-- logs/
      |-- api*.log              (API rolling logs)
      `-- ...

================================================================================
TROUBLESHOOTING
================================================================================

PROBLEM: "Application does not start"

  1. Verify that ports 5084 and 5019 are available.
  2. Check files under logs/ for startup errors.
  3. Ensure firewall prompts were accepted.

PROBLEM: "Browser did not open"

  Open your browser manually and navigate to: http://localhost:5019

PROBLEM: "Cannot access API/Web"

  1. Confirm both processes are running in Task Manager:
     - FamilyFinances.Api.exe
     - FamilyFinances.Web.exe
  2. Re-run Stop and then Start scripts.
  3. Inspect logs/ for errors.

PROBLEM: "Data missing"

  Data is persisted in %LOCALAPPDATA%\FamilyFinances\data\. To back up:
  1. Run Stop FamilyFinances.bat
  2. Copy the entire %LOCALAPPDATA%\FamilyFinances\data\ folder to a safe location
  3. Restart with Start FamilyFinances.bat

================================================================================
PORTS
================================================================================

  API:  http://localhost:5084
  Web:  http://localhost:5019

================================================================================
SECURITY NOTES
================================================================================

- This package is intended for local machine usage.
- Data remains in %LOCALAPPDATA%\FamilyFinances\data\ by default.
- Logs contain operational details and should be protected.

================================================================================
SUPPORT
================================================================================

GitHub: https://github.com/carlos-aisa/FamilyFinances

================================================================================
LICENSE
================================================================================

Provided as-is. See LICENSE in the source repository.

================================================================================

Maintainer rollback note:
- To revert to the legacy distribution layout, restore scripts/workflow that
  publish into separate full api/ and web/ runtime trees.
