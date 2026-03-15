[CmdletBinding()]
param(
    [Parameter()] [string]$SiteName = "FamilyFinances.Web",
    [Parameter()] [string]$AppPoolName = "FamilyFinances.Web.AppPool"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator

Import-Module WebAdministration

$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($null -ne $site) {
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue | Out-Null
    Remove-Website -Name $SiteName
}

if (Test-Path ("IIS:\AppPools\$AppPoolName")) {
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue | Out-Null
    Remove-WebAppPool -Name $AppPoolName
}

[pscustomobject]@{
    SiteName    = $SiteName
    AppPoolName = $AppPoolName
    Removed     = $true
}
