@echo off
setlocal enabledelayedexpansion

echo ========================================
echo FamilyFinances - Starting...
echo ========================================
echo.

REM Set production environment
set ASPNETCORE_ENVIRONMENT=Production

REM Get the script directory
set ROOT_DIR=%~dp0
cd /d "%ROOT_DIR%"

REM Create required directories
if not exist "data" mkdir data
if not exist "logs" mkdir logs
set "FF_HOME=%LOCALAPPDATA%\FamilyFinances"
if not defined LOCALAPPDATA set "FF_HOME=%ROOT_DIR%data"
if not exist "%FF_HOME%" mkdir "%FF_HOME%"
if not exist "%FF_HOME%\data" mkdir "%FF_HOME%\data"

echo [1/5] Folders ready (logs, database storage)
echo      Database path: %FF_HOME%\data\familyfinances.db

REM Check if API is already running
tasklist /FI "IMAGENAME eq FamilyFinances.Api.exe" 2>NUL | find /I /N "FamilyFinances.Api.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [2/5] API is already running
    goto :start_web
)

REM Start API in background (minimized)
echo [2/5] Starting API on http://localhost:5084...
set "FF_CONFIG_ROOT=%ROOT_DIR%config\api"
set "ConnectionStrings__Default=Data Source=%FF_HOME%\data\familyfinances.db"
start /D "%ROOT_DIR%" /min "FamilyFinances API" "%ROOT_DIR%FamilyFinances.Api.exe"

REM Save API PID for later shutdown
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq FamilyFinances.Api.exe" /NH') do (
    set API_PID=%%a
    goto :pid_saved
)
:pid_saved
echo %API_PID% > "%ROOT_DIR%api.pid"

REM Wait for API health check
echo [3/5] Waiting for API to be ready...
set MAX_RETRIES=30
set RETRY_COUNT=0

:wait_api
timeout /t 1 /nobreak >NUL
powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:5084/health' -TimeoutSec 2 -UseBasicParsing; if ($response.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }" >NUL 2>&1

if %ERRORLEVEL% EQU 0 (
    echo [3/5] API is ready!
    goto :start_web
)

set /a RETRY_COUNT+=1
if %RETRY_COUNT% LSS %MAX_RETRIES% (
    echo Waiting... (%RETRY_COUNT%/%MAX_RETRIES%)
    goto :wait_api
)

echo.
echo ERROR: API did not start within 30 seconds
echo Please check logs\api*.log for details
pause
exit /b 1

:start_web
REM Check if Web is already running
tasklist /FI "IMAGENAME eq FamilyFinances.Web.exe" 2>NUL | find /I /N "FamilyFinances.Web.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo [4/5] Web is already running
    goto :open_browser
)

REM Start Web in background (minimized)
echo [4/5] Starting Web on http://localhost:5019...
set "FF_CONFIG_ROOT=%ROOT_DIR%config\web"
start /D "%ROOT_DIR%" /min "FamilyFinances Web" "%ROOT_DIR%FamilyFinances.Web.exe"

REM Save Web PID for later shutdown
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq FamilyFinances.Web.exe" /NH') do (
    set WEB_PID=%%a
    goto :web_pid_saved
)
:web_pid_saved
echo %WEB_PID% > "%ROOT_DIR%web.pid"

REM Brief wait for Web to start
timeout /t 3 /nobreak >NUL

:open_browser
REM Open browser
echo [5/5] Opening browser...
start http://localhost:5019

echo.
echo ========================================
echo FamilyFinances is running!
echo ========================================
echo.
echo   Web Interface: http://localhost:5019
echo   API:           http://localhost:5084
echo.
echo To stop, run: Stop FamilyFinances.bat
echo.
echo Press any key to minimize this window...
pause >NUL
