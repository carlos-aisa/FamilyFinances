[CmdletBinding()]
param(
    [Parameter()] [string]$InstallRoot = "",
    [Parameter()] [string]$RuntimeRoot = "",
    [Parameter()] [int]$ApiPort = 0,
    [Parameter()] [int]$WebPort = 0,
    [Parameter()] [int]$EnableIisIfMissing = 1,
    [Parameter()] [int]$RotateJwtKey = 0
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
if ($ApiPort -le 0) {
    $ApiPort = [int]$defaults.ApiPortLocal
}
if ($WebPort -le 0) {
    $WebPort = [int]$defaults.WebPortLocal
}

$enableIis = $EnableIisIfMissing -eq 1
$rotateJwt = $RotateJwtKey -eq 1

[Environment]::SetEnvironmentVariable("FF_HOSTOPS_SCRIPTS_ROOT", $scriptsRoot, "Machine")
[Environment]::SetEnvironmentVariable("FF_HOSTOPS_SCRIPTS_ROOT", $scriptsRoot, "Process")

$hostingBundlePath = Join-Path $InstallRoot $defaults.HostingBundleRelativePath

& (Join-Path $scriptsRoot "Assert-InstallerPreconditions.ps1") `
    -EnableIisIfMissing:$enableIis `
    -HostingBundlePath $hostingBundlePath | Out-Null

& (Join-Path $scriptsRoot "Set-ManagedRuntimeConfig.ps1") `
    -PayloadRoot $InstallRoot `
    -RuntimeRoot $RuntimeRoot `
    -ApiPort $ApiPort `
    -WebPort $WebPort `
    -RotateJwtKey:$rotateJwt | Out-Null

$apiExePath = Join-Path $InstallRoot $defaults.ApiExeName

& (Join-Path $scriptsRoot "Register-ApiService.ps1") `
    -ApiExePath $apiExePath `
    -ServiceName $defaults.ApiServiceName `
    -ApiPort $ApiPort | Out-Null

& (Join-Path $scriptsRoot "Configure-WebIisSite.ps1") `
    -PhysicalPath $InstallRoot `
    -SiteName $defaults.SiteName `
    -AppPoolName $defaults.AppPoolName `
    -LocalPort $WebPort | Out-Null

& (Join-Path $scriptsRoot "Test-PostInstallHealth.ps1") `
    -ApiPort $ApiPort `
    -WebPort $WebPort | Out-Null
