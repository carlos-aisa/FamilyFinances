param(
    [string]$ApiPublishDir = "publish_compare_api",
    [string]$WebPublishDir = "publish_compare_web"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-RelativeFileMap {
    param(
        [Parameter(Mandatory = $true)] [string]$Root
    )

    $resolvedRoot = (Resolve-Path $Root).Path
    $prefixLength = $resolvedRoot.Length + 1

    $map = @{}
    Get-ChildItem $resolvedRoot -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($prefixLength)
        $normalized = $relativePath -replace "\\", "/"
        $map[$normalized] = $_.FullName
    }

    return $map
}

if (!(Test-Path $ApiPublishDir)) {
    Write-Error "Missing API publish folder: $ApiPublishDir"
    exit 1
}

if (!(Test-Path $WebPublishDir)) {
    Write-Error "Missing Web publish folder: $WebPublishDir"
    exit 1
}

$apiMap = Get-RelativeFileMap -Root $ApiPublishDir
$webMap = Get-RelativeFileMap -Root $WebPublishDir

$apiFiles = @($apiMap.Keys | Sort-Object)
$webFiles = @($webMap.Keys | Sort-Object)

$common = @($apiFiles | Where-Object { $webMap.ContainsKey($_) } | Sort-Object)
$identical = New-Object System.Collections.Generic.List[string]
$different = New-Object System.Collections.Generic.List[string]

foreach ($relative in $common) {
    $apiHash = (Get-FileHash $apiMap[$relative] -Algorithm SHA256).Hash
    $webHash = (Get-FileHash $webMap[$relative] -Algorithm SHA256).Hash

    if ($apiHash -eq $webHash) {
        $identical.Add($relative)
    }
    else {
        $different.Add($relative)
    }
}

Write-Output "API_FILES=$($apiFiles.Count)"
Write-Output "WEB_FILES=$($webFiles.Count)"
Write-Output "COMMON=$($common.Count)"
Write-Output "IDENTICAL=$($identical.Count)"
Write-Output "DIFFERENT=$($different.Count)"

Write-Output "DIFFERENT_LIST:"
if ($different.Count -eq 0) {
    Write-Output "(none)"
}
else {
    $different | Sort-Object | ForEach-Object { Write-Output $_ }
}
