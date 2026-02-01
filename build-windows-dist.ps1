# Build and Package FamilyFinances for Windows Distribution
# Version: 0.6.7

param(
    [string]$Version = "0.6.7",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FamilyFinances Windows Distribution Builder" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$RootDir = $PSScriptRoot
$SrcDir = Join-Path $RootDir "src"
$DistBaseDir = Join-Path $RootDir "dist"
$DistDir = Join-Path $DistBaseDir "FamilyFinances-v$Version-win-x64"
$ApiProject = Join-Path $SrcDir "FamilyFinances.Api\FamilyFinances.Api.csproj"
$WebProject = Join-Path $SrcDir "FamilyFinances.Web\FamilyFinances.Web.csproj"

# Clean previous distribution
if (Test-Path $DistDir) {
    Write-Host "[1/8] Cleaning previous distribution..." -ForegroundColor Yellow
    Remove-Item $DistDir -Recurse -Force
}

# Create distribution structure
Write-Host "[2/8] Creating distribution folders..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\api" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\web" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\data" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistDir\logs" -Force | Out-Null

# Publish API
Write-Host "[3/8] Publishing API (win-x64, self-contained)..." -ForegroundColor Yellow
$ApiPublishDir = Join-Path $RootDir "publish-temp\api"
dotnet publish $ApiProject `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $ApiPublishDir `
    /p:PublishTrimmed=false `
    /p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: API publish failed" -ForegroundColor Red
    exit 1
}

# Publish Web
Write-Host "[4/8] Publishing Web (win-x64, self-contained)..." -ForegroundColor Yellow
$WebPublishDir = Join-Path $RootDir "publish-temp\web"
dotnet publish $WebProject `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $WebPublishDir `
    /p:PublishTrimmed=false `
    /p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Web publish failed" -ForegroundColor Red
    exit 1
}

# Copy published files to distribution
Write-Host "[5/8] Copying published files..." -ForegroundColor Yellow
Copy-Item -Path "$ApiPublishDir\*" -Destination "$DistDir\api" -Recurse -Force
Copy-Item -Path "$WebPublishDir\*" -Destination "$DistDir\web" -Recurse -Force

# Copy scripts and documentation
Write-Host "[6/8] Copying scripts and documentation..." -ForegroundColor Yellow
Copy-Item -Path "$DistBaseDir\Start FamilyFinances.bat" -Destination $DistDir -Force
Copy-Item -Path "$DistBaseDir\Stop FamilyFinances.bat" -Destination $DistDir -Force
Copy-Item -Path "$DistBaseDir\README.txt" -Destination $DistDir -Force

# Clean up temp publish directories
Write-Host "[7/8] Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item (Join-Path $RootDir "publish-temp") -Recurse -Force -ErrorAction SilentlyContinue

# Create ZIP archive
Write-Host "[8/8] Creating ZIP archive..." -ForegroundColor Yellow
$ZipPath = Join-Path $DistBaseDir "FamilyFinances-v$Version-win-x64.zip"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path $DistDir -DestinationPath $ZipPath -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Distribution folder: $DistDir" -ForegroundColor White
Write-Host "ZIP archive:         $ZipPath" -ForegroundColor White
Write-Host ""
Write-Host "To test locally, navigate to:" -ForegroundColor Yellow
Write-Host "  $DistDir" -ForegroundColor Yellow
Write-Host "And run: Start FamilyFinances.bat" -ForegroundColor Yellow
Write-Host ""
