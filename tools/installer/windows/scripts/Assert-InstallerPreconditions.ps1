[CmdletBinding()]
param(
    [switch]$EnableIisIfMissing
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")

Assert-Administrator

if ($env:OS -ne "Windows_NT") {
    throw "Installer precheck can only run on Windows."
}

$missing = New-Object System.Collections.Generic.List[string]

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $missing.Add("dotnet SDK/runtime")
}

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
        Enable-WindowsOptionalFeature -Online -FeatureName $featureName -All -NoRestart | Out-Null
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
        $missing.Add("IIS module AspNetCoreModuleV2 (.NET Hosting Bundle is required)")
    }
}

if (-not (Get-Module -ListAvailable NetSecurity)) {
    $missing.Add("PowerShell module: NetSecurity")
}

if ($missing.Count -gt 0) {
    throw ("Precheck failed. Missing prerequisites: " + ($missing -join ", "))
}

[pscustomobject]@{
    Ok             = $true
    TimestampUtc   = [DateTime]::UtcNow.ToString("O")
    EnableIis      = [bool]$EnableIisIfMissing
    Preconditions  = "All required prerequisites are available."
}
