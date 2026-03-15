[CmdletBinding()]
param(
    [Parameter()] [string]$InstallRoot = "",
    [Parameter()] [string]$RuntimeRoot = "",
    [Parameter()] [int]$RemoveData = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$installerRoot = Split-Path -Parent $scriptsRoot
. (Join-Path $installerRoot "constants.ps1")

$defaults = Get-InstallerDefaults

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = $installerRoot
}
if ([string]::IsNullOrWhiteSpace($RuntimeRoot)) {
    $RuntimeRoot = [Environment]::GetEnvironmentVariable("FF_RUNTIME_ROOT", "Machine")
}
if ([string]::IsNullOrWhiteSpace($RuntimeRoot)) {
    $RuntimeRoot = $defaults.RuntimeRoot
}

$shouldRemoveData = $RemoveData -eq 1

& (Join-Path $scriptsRoot "Set-LanAccess.ps1") `
    -Enabled:$false `
    -SiteName $defaults.SiteName `
    -FirewallRuleName $defaults.LanFirewallRuleName `
    -HttpsPort $defaults.LanHttpsPort | Out-Null

& (Join-Path $scriptsRoot "Unregister-ApiService.ps1") `
    -ServiceName $defaults.ApiServiceName | Out-Null

& (Join-Path $scriptsRoot "Remove-WebIisSite.ps1") `
    -SiteName $defaults.SiteName `
    -AppPoolName $defaults.AppPoolName | Out-Null

if ($shouldRemoveData -and (Test-Path $RuntimeRoot)) {
    Remove-Item $RuntimeRoot -Recurse -Force
}

[Environment]::SetEnvironmentVariable("FF_HOSTOPS_SCRIPTS_ROOT", $null, "Machine")
[Environment]::SetEnvironmentVariable("FF_RUNTIME_ROOT", $null, "Machine")
