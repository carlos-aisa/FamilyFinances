[CmdletBinding()]
param(
    [Parameter()] [string]$Version = "0.9.7",
    [Parameter()] [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$distRoot = Join-Path $repoRoot "dist"
$sourceDistDir = Join-Path $distRoot ("FamilyFinances-v{0}-win-x64" -f $Version)
$msiLayoutDir = Join-Path $distRoot ("FamilyFinances-v{0}-win-x64-msi-layout" -f $Version)
$installerMsi = Join-Path $distRoot ("FamilyFinances-v{0}-win-x64.msi" -f $Version)
$setupBootstrapper = Join-Path $distRoot ("FamilyFinances-v{0}-win-x64-setup.exe" -f $Version)
$wixProject = Join-Path $PSScriptRoot "wix\FamilyFinances.Installer.wixproj"
$wixBootstrapperProject = Join-Path $PSScriptRoot "bootstrapper\FamilyFinances.Bootstrapper.wixproj"
$hostingBundleCacheDir = Join-Path $env:TEMP "FamilyFinances-Installer-Prereqs"
$hostingBundleSource = Join-Path $hostingBundleCacheDir "dotnet-hosting-9.0-win.exe"
$hostingBundleUrl = "https://aka.ms/dotnet/9.0/dotnet-hosting-win.exe"

function Convert-ToMsiVersion {
    param([Parameter(Mandatory = $true)] [string]$InputVersion)

    $parts = ($InputVersion -split "[^0-9]") | Where-Object { $_ -ne "" }
    if ($parts.Count -lt 3) {
        throw "MSI requires semantic version with at least major.minor.patch. Received '$InputVersion'."
    }

    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]

    return "$major.$minor.$patch"
}

Push-Location $repoRoot
try {
    & ".\build-windows-dist.ps1" -Version $Version -Configuration $Configuration | Out-Null

    & (Join-Path $PSScriptRoot "scripts\Publish-MsiLayout.ps1") `
        -Version $Version `
        -SourceDistDir $sourceDistDir `
        -MsiLayoutDir $msiLayoutDir | Out-Null

    if (Test-Path $installerMsi) {
        Remove-Item $installerMsi -Force
    }

    $msiVersion = Convert-ToMsiVersion -InputVersion $Version
    $msiOutputName = "FamilyFinances-v{0}-win-x64" -f $Version
    dotnet build $wixProject -c $Configuration `
        /p:ProductVersion=$msiVersion `
        /p:MsiSourceDir=$msiLayoutDir `
        /p:OutputName=$msiOutputName `
        /p:OutputPath="$distRoot\" | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "WiX build failed with exit code $LASTEXITCODE."
    }

    $candidateMsi = Get-ChildItem -Path (Join-Path $PSScriptRoot "wix") -Recurse -Filter ("FamilyFinances-v{0}-win-x64.msi" -f $Version) |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $candidateMsi) {
        throw "MSI artifact not found after WiX build."
    }

    if (-not (Test-Path $installerMsi)) {
        $copied = $false
        for ($attempt = 1; $attempt -le 5 -and -not $copied; $attempt++) {
            try {
                Copy-Item -Path $candidateMsi.FullName -Destination $installerMsi -Force
                $copied = $true
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }
    }

    if (-not (Test-Path $installerMsi)) {
        throw "MSI artifact not found at expected path: $installerMsi"
    }

    if (Test-Path $setupBootstrapper) {
        Remove-Item $setupBootstrapper -Force
    }

    if (-not (Test-Path $hostingBundleCacheDir)) {
        New-Item -ItemType Directory -Path $hostingBundleCacheDir -Force | Out-Null
    }

    if (-not (Test-Path $hostingBundleSource)) {
        Write-Host "Downloading .NET 9 Hosting Bundle for web bootstrapper build..."
        Invoke-WebRequest -Uri $hostingBundleUrl -OutFile $hostingBundleSource | Out-Null
    }

    $setupOutputName = "FamilyFinances-v{0}-win-x64-setup" -f $Version
    dotnet build $wixBootstrapperProject -c $Configuration `
        /p:ProductVersion=$msiVersion `
        /p:InstallerMsiPath=$installerMsi `
        /p:HostingBundleSourcePath=$hostingBundleSource `
        /p:OutputName=$setupOutputName `
        /p:OutputPath="$distRoot\" | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "WiX bootstrapper build failed with exit code $LASTEXITCODE."
    }

    $candidateSetupExe = Get-ChildItem -Path (Join-Path $PSScriptRoot "bootstrapper") -Recurse -Filter ("FamilyFinances-v{0}-win-x64-setup.exe" -f $Version) |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $candidateSetupExe) {
        throw "Bootstrapper executable not found after WiX build."
    }

    if (-not (Test-Path $setupBootstrapper)) {
        $copiedSetup = $false
        for ($attempt = 1; $attempt -le 5 -and -not $copiedSetup; $attempt++) {
            try {
                Copy-Item -Path $candidateSetupExe.FullName -Destination $setupBootstrapper -Force
                $copiedSetup = $true
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }
    }

    if (-not (Test-Path $setupBootstrapper)) {
        throw "Bootstrapper executable not found at expected path: $setupBootstrapper"
    }

    [pscustomobject]@{
        Version            = $Version
        MsiLayoutDir       = $msiLayoutDir
        InstallerMsi       = $installerMsi
        InstallerSetupExe  = $setupBootstrapper
        BuildConfiguration = $Configuration
    } | ConvertTo-Json -Depth 4
}
finally {
    Pop-Location
}
