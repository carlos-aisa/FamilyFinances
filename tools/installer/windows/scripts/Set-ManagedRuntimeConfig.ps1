[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$PayloadRoot,
    [Parameter(Mandatory = $true)] [string]$RuntimeRoot,
    [Parameter()] [int]$ApiPort = 5084,
    [Parameter()] [int]$WebPort = 5019,
    [switch]$RotateJwtKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "..\constants.ps1")

function New-SecureJwtKey {
    $bytes = New-Object byte[] 64
    $fillMethod = [System.Security.Cryptography.RandomNumberGenerator].GetMethod(
        "Fill",
        [Type[]]@([byte[]])
    )

    if ($null -ne $fillMethod) {
        [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    }
    else {
        $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try {
            $rng.GetBytes($bytes)
        }
        finally {
            if ($null -ne $rng) {
                $rng.Dispose()
            }
        }
    }

    return [Convert]::ToBase64String($bytes)
}

function Get-ExistingJwtKey {
    param([string]$ApiConfigPath)

    if (-not (Test-Path $ApiConfigPath)) {
        return $null
    }

    try {
        $existing = Get-Content -Raw $ApiConfigPath | ConvertFrom-Json
        return $existing.Jwt.Key
    }
    catch {
        return $null
    }
}

$defaults = Get-InstallerDefaults
$configRoot = Join-Path $RuntimeRoot "config"
$apiConfigRoot = Join-Path $configRoot "api"
$webConfigRoot = Join-Path $configRoot "web"

New-Item -ItemType Directory -Path $apiConfigRoot -Force | Out-Null
New-Item -ItemType Directory -Path $webConfigRoot -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $RuntimeRoot "data") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $RuntimeRoot "logs") -Force | Out-Null

$payloadApiConfig = Join-Path $PayloadRoot "config\api"
$payloadWebConfig = Join-Path $PayloadRoot "config\web"

if (-not (Test-Path $payloadApiConfig)) {
    throw "Payload API config directory not found: $payloadApiConfig"
}
if (-not (Test-Path $payloadWebConfig)) {
    throw "Payload Web config directory not found: $payloadWebConfig"
}

Copy-Item -Path (Join-Path $payloadApiConfig "*") -Destination $apiConfigRoot -Force
Copy-Item -Path (Join-Path $payloadWebConfig "*") -Destination $webConfigRoot -Force

# IIS needs web.config in the site physical path (InstallRoot) to load AspNetCoreModuleV2.
$webConfigSource = Join-Path $webConfigRoot "web.config"
$webConfigTarget = Join-Path $PayloadRoot "web.config"
if (-not (Test-Path $webConfigSource)) {
    throw "Missing Web IIS configuration file: $webConfigSource"
}
Copy-Item -Path $webConfigSource -Destination $webConfigTarget -Force

$apiProdPath = Join-Path $apiConfigRoot "appsettings.Production.json"
$webProdPath = Join-Path $webConfigRoot "appsettings.Production.json"

$apiProd = Get-Content -Raw $apiProdPath | ConvertFrom-Json
$webProd = Get-Content -Raw $webProdPath | ConvertFrom-Json

$existingJwt = Get-ExistingJwtKey -ApiConfigPath $apiProdPath
$defaultJwt = $defaults.DefaultJwtKey
$mustRotate = $RotateJwtKey -or [string]::IsNullOrWhiteSpace($existingJwt) -or $existingJwt -eq $defaultJwt -or $existingJwt.Length -lt 32
$jwtKey = if ($mustRotate) { New-SecureJwtKey } else { $existingJwt }

if ($null -eq $apiProd.Jwt) {
    $apiProd | Add-Member -NotePropertyName "Jwt" -NotePropertyValue ([pscustomobject]@{}) -Force
}

$apiProd.Jwt.Key = $jwtKey
$apiProd.ConnectionStrings.Default = "Data Source=$RuntimeRoot\data\familyfinances.db"
$apiProd.Kestrel.Endpoints.Http.Url = "http://127.0.0.1:$ApiPort"

$webProd.Api.BaseUrl = "http://127.0.0.1:$ApiPort/"
if ($null -ne $webProd.Kestrel -and $null -ne $webProd.Kestrel.Endpoints -and $null -ne $webProd.Kestrel.Endpoints.Http) {
    $webProd.Kestrel.Endpoints.Http.Url = "http://127.0.0.1:$WebPort"
}

$apiProd | ConvertTo-Json -Depth 50 | Set-Content $apiProdPath -Encoding utf8
$webProd | ConvertTo-Json -Depth 50 | Set-Content $webProdPath -Encoding utf8

[Environment]::SetEnvironmentVariable("FF_RUNTIME_ROOT", $RuntimeRoot, "Machine")
[Environment]::SetEnvironmentVariable("FF_RUNTIME_ROOT", $RuntimeRoot, "Process")

# Avoid stale machine-wide overrides breaking the packaged Web configuration.
[Environment]::SetEnvironmentVariable("Api__BaseUrl", $null, "Machine")
[Environment]::SetEnvironmentVariable("Api__BaseUrl", $null, "Process")

[pscustomobject]@{
    RuntimeRoot   = $RuntimeRoot
    ApiConfigRoot = $apiConfigRoot
    WebConfigRoot = $webConfigRoot
    ApiPort       = $ApiPort
    WebPort       = $WebPort
    JwtRotated    = [bool]$mustRotate
}
