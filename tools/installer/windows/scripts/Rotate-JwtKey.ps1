[CmdletBinding()]
param(
    [Parameter()] [string]$RuntimeRoot = (Join-Path $env:ProgramData "FamilyFinances"),
    [Parameter()] [int]$ApiPort = 5084,
    [Parameter()] [int]$WebPort = 5019
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$payloadFallback = Join-Path $env:ProgramFiles "FamilyFinances"
if (-not (Test-Path $payloadFallback)) {
    throw "Install payload path not found: $payloadFallback"
}

& (Join-Path $PSScriptRoot "Set-ManagedRuntimeConfig.ps1") `
    -PayloadRoot $payloadFallback `
    -RuntimeRoot $RuntimeRoot `
    -ApiPort $ApiPort `
    -WebPort $WebPort `
    -RotateJwtKey | Out-Null

[pscustomobject]@{
    Rotated     = $true
    RuntimeRoot = $RuntimeRoot
    TimestampUtc = [DateTime]::UtcNow.ToString("O")
}
