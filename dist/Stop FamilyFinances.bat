@echo off
echo ========================================
echo FamilyFinances - Stopping...
echo ========================================
echo.

REM Get the script directory
set ROOT_DIR=%~dp0
cd /d "%ROOT_DIR%"

REM Stop Web process
echo [1/2] Stopping Web...
tasklist /FI "IMAGENAME eq FamilyFinances.Web.exe" 2>NUL | find /I /N "FamilyFinances.Web.exe">NUL
if "%ERRORLEVEL%"=="0" (
    taskkill /IM FamilyFinances.Web.exe /F >NUL 2>&1
    echo Web stopped.
) else (
    echo Web was not running.
)

REM Clean up Web PID file
if exist "%ROOT_DIR%web.pid" del "%ROOT_DIR%web.pid"

REM Stop API process
echo [2/2] Stopping API...
tasklist /FI "IMAGENAME eq FamilyFinances.Api.exe" 2>NUL | find /I /N "FamilyFinances.Api.exe">NUL
if "%ERRORLEVEL%"=="0" (
    taskkill /IM FamilyFinances.Api.exe /F >NUL 2>&1
    echo API stopped.
) else (
    echo API was not running.
)

REM Clean up API PID file
if exist "%ROOT_DIR%api.pid" del "%ROOT_DIR%api.pid"

echo.
echo ========================================
echo FamilyFinances has been stopped.
echo ========================================
echo.
pause
