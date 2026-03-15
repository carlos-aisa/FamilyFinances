[CmdletBinding()]
param(
    [Parameter()] [string]$ServiceName = "FamilyFinances.Api"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")
Assert-Administrator

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -eq $service) {
    return [pscustomobject]@{
        ServiceName = $ServiceName
        Removed     = $false
        Reason      = "Service not found"
    }
}

if ($service.Status -ne "Stopped") {
    Stop-Service -Name $ServiceName -Force
}

& sc.exe delete $ServiceName | Out-Null

[pscustomobject]@{
    ServiceName = $ServiceName
    Removed     = $true
}
