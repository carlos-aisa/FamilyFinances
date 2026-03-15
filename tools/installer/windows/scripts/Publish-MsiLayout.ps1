[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$Version,
    [Parameter(Mandatory = $true)] [string]$SourceDistDir,
    [Parameter(Mandatory = $true)] [string]$MsiLayoutDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path $SourceDistDir)) {
    throw "Source distribution directory not found: $SourceDistDir"
}

if (Test-Path $MsiLayoutDir) {
    Remove-Item $MsiLayoutDir -Recurse -Force
}

New-Item -ItemType Directory -Path $MsiLayoutDir -Force | Out-Null
Copy-Item -Path (Join-Path $SourceDistDir "*") -Destination $MsiLayoutDir -Recurse -Force

$installerToolsRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$scriptsTarget = Join-Path $MsiLayoutDir "installer-scripts"
New-Item -ItemType Directory -Path $scriptsTarget -Force | Out-Null

Copy-Item -Path (Join-Path $installerToolsRoot "constants.ps1") -Destination $MsiLayoutDir -Force
Copy-Item -Path (Join-Path $installerToolsRoot "scripts\*.ps1") -Destination $scriptsTarget -Force

[pscustomobject]@{
    Version      = $Version
    MsiLayoutDir = $MsiLayoutDir
}
