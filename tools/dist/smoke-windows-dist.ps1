param(
    [Parameter(Mandatory = $true)] [string]$ZipPath,
    [string]$ExtractRoot
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (!(Test-Path $ZipPath)) {
    throw "ZIP not found: $ZipPath"
}

if ([string]::IsNullOrWhiteSpace($ExtractRoot)) {
    $zipBaseName = [System.IO.Path]::GetFileNameWithoutExtension($ZipPath)
    $ExtractRoot = Join-Path ([System.IO.Path]::GetDirectoryName((Resolve-Path $ZipPath).Path)) "smoke-extract-$zipBaseName"
}

if (Test-Path $ExtractRoot) {
    Remove-Item $ExtractRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $ExtractRoot -Force | Out-Null

Get-Process FamilyFinances.Api -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process FamilyFinances.Web -ErrorAction SilentlyContinue | Stop-Process -Force

Expand-Archive -Path $ZipPath -DestinationPath $ExtractRoot -Force

$zipBaseName = [System.IO.Path]::GetFileNameWithoutExtension((Resolve-Path $ZipPath).Path)
$packageRoot = Join-Path $ExtractRoot $zipBaseName
if (!(Test-Path $packageRoot)) {
    throw "Extracted package root not found: $packageRoot"
}

Push-Location $packageRoot
$startConsole = $null
$stopConsole = $null
try {
    $startConsole = Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"`"Start FamilyFinances.bat`"`"" -PassThru

    $apiOk = $false
    for ($i = 0; $i -lt 20; $i++) {
        try {
            $apiResponse = Invoke-WebRequest -Uri "http://localhost:5084/health" -TimeoutSec 3 -UseBasicParsing
            if ($apiResponse.StatusCode -eq 200) {
                $apiOk = $true
                break
            }
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    if (-not $apiOk) {
        throw "API /health did not return 200."
    }

    $webOk = $false
    for ($i = 0; $i -lt 20; $i++) {
        try {
            $webResponse = Invoke-WebRequest -Uri "http://localhost:5019" -TimeoutSec 3 -UseBasicParsing
            if ($webResponse.StatusCode -ge 200 -and $webResponse.StatusCode -lt 500) {
                $webOk = $true
                break
            }
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    if (-not $webOk) {
        throw "Web endpoint did not respond on http://localhost:5019."
    }

    $apiPidPath = Join-Path $packageRoot "api.pid"
    $webPidPath = Join-Path $packageRoot "web.pid"
    $apiPidExists = Test-Path $apiPidPath
    $webPidExists = Test-Path $webPidPath
    if (-not $apiPidExists -or -not $webPidExists) {
        throw "PID files missing. api.pid=$apiPidExists web.pid=$webPidExists"
    }

    $stopConsole = Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"`"Stop FamilyFinances.bat`"`"" -PassThru
    Start-Sleep -Seconds 2

    $apiRunning = @(Get-Process FamilyFinances.Api -ErrorAction SilentlyContinue).Count -gt 0
    $webRunning = @(Get-Process FamilyFinances.Web -ErrorAction SilentlyContinue).Count -gt 0
    $apiPidAfter = Test-Path $apiPidPath
    $webPidAfter = Test-Path $webPidPath

    if ($apiRunning -or $webRunning) {
        throw "Processes still running. API=$apiRunning WEB=$webRunning"
    }

    if ($apiPidAfter -or $webPidAfter) {
        throw "PID files still present after stop. api.pid=$apiPidAfter web.pid=$webPidAfter"
    }

    Write-Output "SMOKE_STATUS=PASS"
    Write-Output "API_HEALTH=200"
    Write-Output "WEB_PORT_5019=REACHABLE"
    Write-Output "PID_FILES_CREATED=TRUE"
    Write-Output "PID_FILES_CLEANED=TRUE"

}
finally {
    if ($startConsole -and -not $startConsole.HasExited) {
        Stop-Process -Id $startConsole.Id -Force -ErrorAction SilentlyContinue
    }

    if ($stopConsole -and -not $stopConsole.HasExited) {
        Stop-Process -Id $stopConsole.Id -Force -ErrorAction SilentlyContinue
    }

    Pop-Location
}
