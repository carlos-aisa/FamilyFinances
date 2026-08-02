[CmdletBinding()]
param(
    [Parameter()] [string]$HostingBundlePath = "",
    [Parameter()] [ValidateSet("Install", "Repair")] [string]$PreferredMode = "Repair"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$installerRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $installerRoot "constants.ps1")

Assert-Administrator

$defaults = Get-InstallerDefaults

function Resolve-HostingBundleExecutable {
    param([string]$ExplicitPath)

    $candidates = New-Object System.Collections.Generic.List[string]

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        $candidates.Add($ExplicitPath)
    }

    $packagedPath = Join-Path $installerRoot $defaults.HostingBundleRelativePath
    $candidates.Add($packagedPath)

    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (Test-Path $candidate) {
            return [pscustomobject]@{
                Path       = (Resolve-Path $candidate).Path
                Downloaded = $false
            }
        }
    }

    $downloadRoot = Join-Path $env:TEMP "FamilyFinances-Installer-Prereqs"
    $downloadPath = Join-Path $downloadRoot $defaults.HostingBundleFileName
    if (-not (Test-Path $downloadRoot)) {
        New-Item -ItemType Directory -Path $downloadRoot -Force | Out-Null
    }

    Invoke-WebRequest -Uri $defaults.HostingBundleDownloadUrl -OutFile $downloadPath | Out-Null

    return [pscustomobject]@{
        Path       = $downloadPath
        Downloaded = $true
    }
}

function Invoke-HostingBundleExecutable {
    param(
        [Parameter(Mandatory = $true)] [string]$ExecutablePath,
        [Parameter(Mandatory = $true)] [ValidateSet("Install", "Repair")] [string]$Mode
    )

    $arguments = if ($Mode -eq "Repair") {
        "/repair /quiet /norestart"
    }
    else {
        "/install /quiet /norestart"
    }

    $process = Start-Process `
        -FilePath $ExecutablePath `
        -ArgumentList $arguments `
        -Wait `
        -PassThru `
        -WindowStyle Hidden

    return [pscustomobject]@{
        Mode     = $Mode
        ExitCode = $process.ExitCode
    }
}

$resolved = Resolve-HostingBundleExecutable -ExplicitPath $HostingBundlePath
$attempts = New-Object System.Collections.Generic.List[string]
$attempts.Add($PreferredMode)
if ($PreferredMode -eq "Repair") {
    $attempts.Add("Install")
}

$lastFailure = $null

foreach ($mode in $attempts) {
    $result = Invoke-HostingBundleExecutable -ExecutablePath $resolved.Path -Mode $mode
    if ($result.ExitCode -eq 0) {
        [pscustomobject]@{
            Path       = $resolved.Path
            Downloaded = [bool]$resolved.Downloaded
            Mode       = $mode
            ExitCode   = 0
        }
        return
    }

    $lastFailure = "Hosting Bundle $mode exited with code $($result.ExitCode)."
}

throw ("Unable to complete Hosting Bundle maintenance. " + $lastFailure)
