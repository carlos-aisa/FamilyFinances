# Build FamilyFinances runtime layout for Windows installer packaging
# Shared-runtime layout (single runtime root, app-specific config directories)

param(
    [string]$Version = "0.6.7",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FamilyFinances Windows Runtime Layout Builder" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$InstallerToolsDir = $PSScriptRoot
$RepoRoot = (Resolve-Path (Join-Path $InstallerToolsDir "..\..\..")).Path

# Paths
$RootDir = $RepoRoot
$SrcDir = Join-Path $RepoRoot "src"
$DistBaseDir = Join-Path $RepoRoot "dist"
$DistDir = Join-Path $DistBaseDir "FamilyFinances-v$Version-win-x64"
$ApiProject = Join-Path $SrcDir "FamilyFinances.Api\FamilyFinances.Api.csproj"
$WebProject = Join-Path $SrcDir "FamilyFinances.Web\FamilyFinances.Web.csproj"
$ApiPublishDir = Join-Path $InstallerToolsDir "publish-temp\api"
$WebPublishDir = Join-Path $InstallerToolsDir "publish-temp\web"

$configFileNames = @(
    "appsettings.json",
    "appsettings.Production.json",
    "appsettings.Development.json",
    "web.config"
)

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)] [string]$Root,
        [Parameter(Mandatory = $true)] [string]$Path
    )

    $resolvedRoot = (Resolve-Path $Root).Path
    $resolvedPath = (Resolve-Path $Path).Path
    $prefixLength = $resolvedRoot.Length
    if ($resolvedPath.Length -gt $prefixLength -and ($resolvedPath[$prefixLength] -eq '\\' -or $resolvedPath[$prefixLength] -eq '/')) {
        $prefixLength += 1
    }

    return $resolvedPath.Substring($prefixLength)
}

function Is-AppConfigFile {
    param([Parameter(Mandatory = $true)] [string]$RelativePath)

    $name = [System.IO.Path]::GetFileName($RelativePath)
    return ($name -like "appsettings*.json" -or $name -eq "web.config")
}

function Update-ApiProductionConfigForSharedRoot {
    param([Parameter(Mandatory = $true)] [string]$AppsettingsPath)

    if (!(Test-Path $AppsettingsPath)) {
        return
    }

    $json = Get-Content -Raw $AppsettingsPath | ConvertFrom-Json

    if ($json.ConnectionStrings -and $json.ConnectionStrings.Default) {
        $json.ConnectionStrings.Default = "Data Source=data\\familyfinances.db"
    }

    if ($json.Serilog -and $json.Serilog.WriteTo) {
        foreach ($sink in $json.Serilog.WriteTo) {
            if ($sink.Name -eq "File" -and $sink.Args) {
                $sink.Args.path = "logs\\api.log"
            }
        }
    }

    $json | ConvertTo-Json -Depth 50 | Set-Content $AppsettingsPath -Encoding utf8
}

function Copy-AppConfigFiles {
    param(
        [Parameter(Mandatory = $true)] [string]$SourceRoot,
        [Parameter(Mandatory = $true)] [string]$DistRoot,
        [Parameter(Mandatory = $true)] [ValidateSet("api", "web")] [string]$App
    )

    $destinationDir = Join-Path $DistRoot "config\$App"
    New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null

    foreach ($name in $configFileNames) {
        $sourcePath = Join-Path $SourceRoot $name
        if (!(Test-Path $sourcePath)) {
            continue
        }

        $destinationPath = Join-Path $destinationDir $name
        Copy-Item -Path $sourcePath -Destination $destinationPath -Force

        if ($App -eq "api" -and $name -eq "appsettings.Production.json") {
            Update-ApiProductionConfigForSharedRoot -AppsettingsPath $destinationPath
        }
    }
}

function Merge-PublishTree {
    param(
        [Parameter(Mandatory = $true)] [string]$SourceRoot,
        [Parameter(Mandatory = $true)] [string]$TargetRoot,
        [Parameter(Mandatory = $true)] [string]$SourceTag,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[string]]$Conflicts
    )

    Get-ChildItem $SourceRoot -Recurse -File | ForEach-Object {
        $relativePath = Get-RelativePath -Root $SourceRoot -Path $_.FullName

        if (Is-AppConfigFile -RelativePath $relativePath) {
            return
        }

        $destinationPath = Join-Path $TargetRoot $relativePath
        $destinationDir = Split-Path -Parent $destinationPath
        if (!(Test-Path $destinationDir)) {
            New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
        }

        if (!(Test-Path $destinationPath)) {
            Copy-Item -Path $_.FullName -Destination $destinationPath -Force
            return
        }

        $sourceHash = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
        $targetHash = (Get-FileHash $destinationPath -Algorithm SHA256).Hash

        if ($sourceHash -eq $targetHash) {
            return
        }

        $Conflicts.Add("$relativePath ($SourceTag)")
    }
}

function Validate-Distribution {
    param([Parameter(Mandatory = $true)] [string]$DistRoot)

    $requiredFiles = @(
        "FamilyFinances.Api.exe",
        "FamilyFinances.Web.exe",
        "FamilyFinances.Api.deps.json",
        "FamilyFinances.Web.deps.json",
        "FamilyFinances.Api.runtimeconfig.json",
        "FamilyFinances.Web.runtimeconfig.json",
        "config\\api\\appsettings.json",
        "config\\api\\appsettings.Production.json",
        "config\\api\\web.config",
        "config\\web\\appsettings.json",
        "config\\web\\appsettings.Production.json",
        "config\\web\\web.config"
    )

    $requiredDirectories = @(
        "data",
        "logs",
        "config",
        "config\\api",
        "config\\web",
        "wwwroot"
    )

    $missing = New-Object System.Collections.Generic.List[string]

    foreach ($file in $requiredFiles) {
        $path = Join-Path $DistRoot $file
        if (!(Test-Path $path)) {
            $missing.Add($file)
        }
    }

    foreach ($dir in $requiredDirectories) {
        $path = Join-Path $DistRoot $dir
        if (!(Test-Path $path)) {
            $missing.Add($dir)
        }
    }

    if ($missing.Count -gt 0) {
        Write-Host "ERROR: Missing required distribution entries:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        exit 1
    }
}

# Clean previous distribution
if (Test-Path $DistDir) {
    Write-Host "[1/8] Cleaning previous distribution..." -ForegroundColor Yellow
    Remove-Item $DistDir -Recurse -Force
}

# Create distribution structure
Write-Host "[2/8] Creating distribution folders..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\data" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\logs" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\config\api" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\config\web" -Force | Out-Null

# Publish API
Write-Host "[3/8] Publishing API (win-x64, self-contained)..." -ForegroundColor Yellow
dotnet publish $ApiProject `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $ApiPublishDir `
    /p:PublishTrimmed=false `
    /p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: API publish failed" -ForegroundColor Red
    throw "API publish failed"
}

# Publish Web
Write-Host "[4/8] Publishing Web (win-x64, self-contained)..." -ForegroundColor Yellow
dotnet publish $WebProject `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $WebPublishDir `
    /p:PublishTrimmed=false `
    /p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Web publish failed" -ForegroundColor Red
    throw "Web publish failed"
}

# Merge publish files into shared runtime root
Write-Host "[5/8] Merging publish outputs into shared runtime root..." -ForegroundColor Yellow
$conflicts = New-Object 'System.Collections.Generic.List[string]'
Merge-PublishTree -SourceRoot $ApiPublishDir -TargetRoot $DistDir -SourceTag "api" -Conflicts $conflicts
Merge-PublishTree -SourceRoot $WebPublishDir -TargetRoot $DistDir -SourceTag "web" -Conflicts $conflicts

if ($conflicts.Count -gt 0) {
    Write-Host "ERROR: Unresolved merge conflicts detected:" -ForegroundColor Red
    $conflicts | Sort-Object -Unique | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    throw "Unresolved merge conflicts detected in runtime layout"
}

# Copy app-specific config files
Write-Host "[6/8] Copying app-specific configuration files..." -ForegroundColor Yellow
Copy-AppConfigFiles -SourceRoot $ApiPublishDir -DistRoot $DistDir -App "api"
Copy-AppConfigFiles -SourceRoot $WebPublishDir -DistRoot $DistDir -App "web"

# Ensure stale fallback ZIP is not carried forward
Write-Host "[7/8] Removing stale ZIP fallback artifact (if present)..." -ForegroundColor Yellow
$ZipPath = Join-Path $DistBaseDir "FamilyFinances-v$Version-win-x64.zip"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# Validate output and cleanup temporary folders
Write-Host "[8/8] Validating distribution contents and cleaning temporary files..." -ForegroundColor Yellow
Validate-Distribution -DistRoot $DistDir
Remove-Item (Join-Path $InstallerToolsDir "publish-temp") -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Runtime layout folder: $DistDir" -ForegroundColor White
Write-Host ""
Write-Host "This layout is consumed by the MSI build pipeline." -ForegroundColor Yellow
Write-Host ""
