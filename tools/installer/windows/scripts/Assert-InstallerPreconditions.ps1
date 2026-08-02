[CmdletBinding()]
param(
    [switch]$EnableIisIfMissing,
    [Parameter()] [string]$HostingBundlePath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")

Assert-Administrator

if ($env:OS -ne "Windows_NT") {
    throw "Installer precheck can only run on Windows."
}

$missing = New-Object System.Collections.Generic.List[string]
$iisChangedThisRun = $false
$restartRequired = $false
$initialRebootState = Test-InstallerRebootPending

$requiredFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-ISAPIFilter",
    "IIS-ISAPIExtensions",
    "IIS-ASPNETCoreModuleV2"
)

foreach ($featureName in $requiredFeatures) {
    $feature = Get-WindowsOptionalFeature -Online -FeatureName $featureName -ErrorAction SilentlyContinue
    if ($null -eq $feature) {
        continue
    }

    if ($feature.State -eq "Enabled") {
        continue
    }

    if ($EnableIisIfMissing) {
        $result = Enable-WindowsOptionalFeature -Online -FeatureName $featureName -All -NoRestart
        $iisChangedThisRun = $true
        if (Test-InstallerRestartNeededValue -Value $result.RestartNeeded) {
            $restartRequired = $true
        }
        continue
    }

    $missing.Add("Windows Feature: $featureName")
}

if (-not (Get-Module -ListAvailable WebAdministration)) {
    $missing.Add("PowerShell module: WebAdministration")
}
else {
    Import-Module WebAdministration -ErrorAction SilentlyContinue | Out-Null
    $aspNetCoreModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
    if ($null -eq $aspNetCoreModule) {
        if ($restartRequired) {
            throw (Get-InstallerRebootMessage -Sources @("IIS feature enablement"))
        }

        $currentRebootState = Test-InstallerRebootPending
        if ($currentRebootState.Pending -and -not $iisChangedThisRun) {
            throw (Get-InstallerRebootMessage -Sources $currentRebootState.Sources)
        }

        & (Join-Path $PSScriptRoot "Invoke-HostingBundleMaintenance.ps1") `
            -HostingBundlePath $HostingBundlePath `
            -PreferredMode "Repair" | Out-Null

        Import-Module WebAdministration -Force -ErrorAction SilentlyContinue | Out-Null
        $aspNetCoreModule = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
        if ($null -eq $aspNetCoreModule) {
            $postMaintenanceRebootState = Test-InstallerRebootPending
            if ($postMaintenanceRebootState.Pending) {
                throw (Get-InstallerRebootMessage -Sources $postMaintenanceRebootState.Sources)
            }

            $missing.Add("IIS module AspNetCoreModuleV2 (Hosting Bundle install/repair did not register the module)")
        }
    }
}

if (-not (Get-Module -ListAvailable NetSecurity)) {
    $missing.Add("PowerShell module: NetSecurity")
}

if ($missing.Count -gt 0) {
    throw ("Precheck failed. Missing prerequisites: " + ($missing -join ", "))
}

[pscustomobject]@{
    Ok                = $true
    TimestampUtc      = [DateTime]::UtcNow.ToString("O")
    EnableIis         = [bool]$EnableIisIfMissing
    IisChangedThisRun = [bool]$iisChangedThisRun
    RebootPendingAtStart = [bool]$initialRebootState.Pending
    Preconditions     = "All required prerequisites are available."
}
