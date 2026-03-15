[CmdletBinding()]
param(
    [Parameter()] [string]$InstallRoot,
    [Parameter()] [string]$RuntimeRoot,
    [switch]$RemoveData
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator

$defaults = Get-InstallerDefaults
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = $defaults.InstallRoot
}
if ([string]::IsNullOrWhiteSpace($RuntimeRoot)) {
    $RuntimeRoot = $defaults.RuntimeRoot
}

if (Test-Path $InstallRoot) {
    Remove-Item $InstallRoot -Recurse -Force
}

if ($RemoveData -and (Test-Path $RuntimeRoot)) {
    Remove-Item $RuntimeRoot -Recurse -Force
}

[pscustomobject]@{
    InstallRootRemoved = -not (Test-Path $InstallRoot)
    RuntimeRootRemoved = if ($RemoveData) { -not (Test-Path $RuntimeRoot) } else { $false }
    RemoveData         = [bool]$RemoveData
}
