param(
    [Parameter(Mandatory = $false)]
    [string] $PolicyPath = ".codex/opsx-gstack-policy.json",

    [Parameter(Mandatory = $false)]
    [string] $PresetsPath = ".codex/opsx-gstack-policy.presets.json"
)

Set-StrictMode -Version Latest

. "$PSScriptRoot/opsx-gstack-policy.ps1"

$policy = Read-JsonFile -Path $PolicyPath
$policyResult = Test-OpsxGstackPolicyObject -Policy $policy

if ($policyResult.Warnings.Count -gt 0) {
    Write-Host "Policy warnings:" -ForegroundColor Yellow
    foreach ($warning in $policyResult.Warnings) {
        Write-Host " - $warning" -ForegroundColor Yellow
    }
}

if (-not $policyResult.IsValid) {
    Write-Host "Policy errors:" -ForegroundColor Red
    foreach ($error in $policyResult.Errors) {
        Write-Host " - $error" -ForegroundColor Red
    }
    exit 1
}

if (Test-Path -LiteralPath $PresetsPath) {
    $presets = Read-JsonFile -Path $PresetsPath
    $presetResult = Test-OpsxGstackPresetsObject -Presets $presets

    if ($presetResult.Warnings.Count -gt 0) {
        Write-Host "Preset warnings:" -ForegroundColor Yellow
        foreach ($warning in $presetResult.Warnings) {
            Write-Host " - $warning" -ForegroundColor Yellow
        }
    }

    if (-not $presetResult.IsValid) {
        Write-Host "Preset errors:" -ForegroundColor Red
        foreach ($error in $presetResult.Errors) {
            Write-Host " - $error" -ForegroundColor Red
        }
        exit 1
    }
}

Write-Host "OpenSpec gstack policy validation passed." -ForegroundColor Green
