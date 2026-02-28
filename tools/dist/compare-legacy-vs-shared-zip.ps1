param(
    [Parameter(Mandatory = $true)] [string]$Version,
    [string]$ApiPublishDir = "publish_compare_api_v2",
    [string]$WebPublishDir = "publish_compare_web_v2"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (!(Test-Path $ApiPublishDir)) {
    throw "API publish directory not found: $ApiPublishDir"
}

if (!(Test-Path $WebPublishDir)) {
    throw "Web publish directory not found: $WebPublishDir"
}

$root = (Get-Location).Path
$legacyRoot = Join-Path $root "dist/legacy-layout-$Version"
$legacyDist = Join-Path $legacyRoot "FamilyFinances-v$Version-win-x64"
$legacyZip = Join-Path $root "dist/FamilyFinances-v$Version-win-x64-legacy.zip"
$newZip = Join-Path $root "dist/FamilyFinances-v$Version-win-x64.zip"

if (!(Test-Path $newZip)) {
    throw "New ZIP not found: $newZip"
}

if (Test-Path $legacyRoot) {
    Remove-Item $legacyRoot -Recurse -Force
}

if (Test-Path $legacyZip) {
    Remove-Item $legacyZip -Force
}

New-Item -ItemType Directory -Path $legacyDist -Force | Out-Null
Copy-Item -Path "dist/Start FamilyFinances.bat" -Destination $legacyDist -Force
Copy-Item -Path "dist/Stop FamilyFinances.bat" -Destination $legacyDist -Force
Copy-Item -Path "dist/README.txt" -Destination $legacyDist -Force
Copy-Item -Path $ApiPublishDir -Destination (Join-Path $legacyDist "api") -Recurse -Force
Copy-Item -Path $WebPublishDir -Destination (Join-Path $legacyDist "web") -Recurse -Force
New-Item -ItemType Directory -Path (Join-Path $legacyDist "data") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $legacyDist "logs") -Force | Out-Null

Compress-Archive -Path $legacyDist -DestinationPath $legacyZip -Force

$legacySize = (Get-Item $legacyZip).Length
$newSize = (Get-Item $newZip).Length
$savedBytes = $legacySize - $newSize
$savedPercent = [math]::Round((($savedBytes / $legacySize) * 100), 2)

Write-Output "LEGACY_ZIP=$legacyZip"
Write-Output "NEW_ZIP=$newZip"
Write-Output "LEGACY_BYTES=$legacySize"
Write-Output "NEW_BYTES=$newSize"
Write-Output "SAVED_BYTES=$savedBytes"
Write-Output "SAVED_PERCENT=$savedPercent"
