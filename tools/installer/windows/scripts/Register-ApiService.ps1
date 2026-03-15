[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$ApiExePath,
    [Parameter()] [string]$ServiceName = "FamilyFinances.Api",
    [Parameter()] [int]$ApiPort = 5084
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator

if (-not (Test-Path $ApiExePath)) {
    throw "API executable not found: $ApiExePath"
}

$binPath = "`"$ApiExePath`" --urls http://127.0.0.1:$ApiPort"
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($null -eq $service) {
    New-Service -Name $ServiceName -BinaryPathName $binPath -DisplayName $ServiceName -StartupType Automatic | Out-Null
}
else {
    & sc.exe config $ServiceName start= auto binPath= "$binPath" | Out-Null
}

$updated = Get-Service -Name $ServiceName
if ($updated.Status -eq "Running") {
    Restart-Service -Name $ServiceName -Force
}
else {
    Start-Service -Name $ServiceName
}

[pscustomobject]@{
    ServiceName = $ServiceName
    ApiExePath  = $ApiExePath
    ApiPort     = $ApiPort
    Status      = (Get-Service -Name $ServiceName).Status.ToString()
}
